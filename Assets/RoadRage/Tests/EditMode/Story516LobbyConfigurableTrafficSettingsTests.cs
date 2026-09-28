using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using RoadRage.App;
using RoadRage.App.Lobby;
using RoadRage.App.Run;
using RoadRage.Features.Lobby;
using RoadRage.Features.Online;
using RoadRage.Features.Run;
using RoadRage.Features.UI;
using RoadRage.Features.Vehicles;
using RoadRage.Shared.Domain;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RoadRage.Tests.EditMode
{
    /// <summary>
    /// Couvre la Story 5.16 : reglages de trafic configurables depuis le lobby (effectif de vehicules IA
    /// et nombre de jeteurs de detritus), bornes authorées dans le TrafficSettingsDef, publication par le
    /// chemin de la Story 2.4 et resolution hote-owned dans NetworkedRunState.
    ///
    /// La couche service/Def/snapshot est eprouvee par le comportement reel avec une fausse plateforme de
    /// lobby (aucun client Steam requis). La couture LobbyFlowController est eprouvee sur une instance
    /// reelle montee a froid (GameObject inactif : Awake n'a pas tourne, les dependances sont injectees
    /// par reflexion) : bornes, refus, publication, repli client, amorcage depuis le Def et assemblage
    /// des arguments d'affichage sont donc exerces, pas seulement lus.
    ///
    /// L'Awake complet reste sonde par reflexion pour ne pas initialiser Steamworks. Le transfert
    /// lobby -> NetworkedRunState, lui, est exerce avec un bootstrap inactif et des services factices.
    /// </summary>
    [Category("Core")]
    public sealed class Story516LobbyConfigurableTrafficSettingsTests
    {
        private RoadRageBootstrap testBootstrap;

        private RoadRageBootstrap previousBootstrap;

        private const uint TestAppId = 480;

        private const string SpawnerSourcePath = "Assets/RoadRage/App/Run/PortalTrafficSpawner.cs";

        private const string FlowControllerSourcePath = "Assets/RoadRage/App/Lobby/LobbyFlowController.cs";

        private const string MvpRunScenePath = "Assets/RoadRage/App/Scenes/MVP_Run.unity";

        private const string MainMenuLobbyScenePath = "Assets/RoadRage/App/Scenes/MainMenuLobby.unity";

        private const string TrafficSettingsDefAssetPath = "Assets/RoadRage/ScriptableObjects/Vehicles/TrafficSettingsDef_Default.asset";

        [TearDown]
        public void TearDownInjectedBootstrap()
        {
            if (testBootstrap == null)
            {
                return;
            }

            var instanceField = typeof(RoadRageBootstrap).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            UnityEngine.Object.DestroyImmediate(testBootstrap.gameObject);
            instanceField.SetValue(null, previousBootstrap);
            testBootstrap = null;
            previousBootstrap = null;
        }

        // --- TrafficSettingsDef : bornes authorées -----------------------------------------------

        [Test]
        public void TrafficSettingsDefClampsLitterThrowersToItsAuthoredBounds()
        {
            var def = BuildDef(8, 2, 0, 8);

            Assert.That(def.ClampLitterThrowers(50), Is.EqualTo(8));
            Assert.That(def.ClampLitterThrowers(-3), Is.EqualTo(0));
            Assert.That(def.ClampLitterThrowers(3), Is.EqualTo(3));
            Assert.That(def.DefaultLitterThrowers, Is.EqualTo(2), "le defaut authore est deja ramene dans ses bornes");
        }

        [Test]
        public void TrafficSettingsDefRejectsLitterThrowersOutsideItsOwnBounds()
        {
            var def = BuildDef(8, 9, 0, 8);
            Assert.That(def.TryValidate(out var error), Is.False, "un defaut hors bornes est invalide");
            Assert.That(error, Does.Contain("DefaultLitterThrowers"));

            var inverted = BuildDef(8, 2, 5, 3);
            Assert.That(inverted.TryValidate(out var invertedError), Is.False);
            Assert.That(invertedError, Does.Contain("MaxLitterThrowers"));

            Assert.That(BuildDef(8, 2, 0, 8).TryValidate(out _), Is.True, "les bornes authorées par defaut sont valides");
        }

        [Test]
        public void TrafficSettingsDefDoesNotEnforceTheLittererInvariantWhichDependsOnTheSession()
        {
            // L'invariant "jeteurs <= effectif" depend de la valeur de session : il ne peut pas etre
            // authoré, et le Def ne doit donc pas le pretendre. Il vit dans la valeur de session.
            var def = BuildDef(4, 2, 0, 8);

            Assert.That(def.TryValidate(out _), Is.True, "un max de jeteurs superieur a l'effectif cible reste authorable");
            Assert.That(def.ClampLitterThrowers(8), Is.EqualTo(8));
        }

        // --- MatchSettings : une seule source de session ------------------------------------------

        [Test]
        public void MatchSettingsCarriesBothTrafficValuesAndStaysAPlainClass()
        {
            var settings = new MatchSettings();

            Assert.That(settings.AiVehicleTargetCount, Is.EqualTo(SessionTrafficValue.Unresolved));
            Assert.That(settings.LitterThrowerCount, Is.EqualTo(SessionTrafficValue.Unresolved));

            settings.AiVehicleTargetCount = 12;
            settings.LitterThrowerCount = 3;
            Assert.That(settings.AiVehicleTargetCount, Is.EqualTo(12));
            Assert.That(settings.LitterThrowerCount, Is.EqualTo(3));

            Assert.That(typeof(MonoBehaviour).IsAssignableFrom(typeof(MatchSettings)), Is.False);
            Assert.That(typeof(ScriptableObject).IsAssignableFrom(typeof(MatchSettings)), Is.False);
        }

        [Test]
        public void TheUnresolvedSentinelIsDistinctFromEveryLegalAuthoredValue()
        {
            var def = BuildDef(8, 2, 0, 8);

            Assert.That(def.ClampTargetPopulation(SessionTrafficValue.Unresolved), Is.EqualTo(0),
                "le clamp ramene le sentinel dans les bornes ; c'est pourquoi il est teste avant le clamp");
            Assert.That(SessionTrafficValue.Unresolved, Is.LessThan(0), "0 est une valeur authorable, le sentinel ne peut pas l'etre");
        }

        // --- Service de roster : publication et propagation ---------------------------------------

        [Test]
        public void PublishTrafficSettingsOnlyReachesThePlatformWhenAHostRoomIsOpen()
        {
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);

            roster.PublishTrafficSettings(12, 3);
            Assert.That(lobbyPlatform.SetTrafficSettingsCallCount, Is.EqualTo(0),
                "sans room hote ouverte, aucune publication de reglages de trafic");

            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            roster.PublishTrafficSettings(12, 3);
            Assert.That(lobbyPlatform.SetTrafficSettingsCallCount, Is.EqualTo(1));
            Assert.That(lobbyPlatform.LastAiVehicleTargetCount, Is.EqualTo(12));
            Assert.That(lobbyPlatform.LastLitterThrowerCount, Is.EqualTo(3));
        }

        [Test]
        public void TickRaisesRosterChangedWhenOnlyTheTrafficSettingsChange()
        {
            // Piege verifie dans le code : SnapshotsEqual ne comparait que HasLobby/OwnerId/Difficulty/
            // RunLaunchRequested/membres. Sans comparaison des reglages de trafic, un changement publie
            // par l'hote n'aurait jamais atteint l'invite.
            var roster = BuildService(out var lobbyPlatform, out var lobbyRoom, out _);
            lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
            lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
            }, false, 8, 2);
            roster.Tick();

            var raiseCount = 0;
            roster.RosterChanged += _ => raiseCount++;
            roster.Tick();
            Assert.That(raiseCount, Is.EqualTo(0), "instantane identique : aucun evenement");

            lobbyPlatform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new[]
            {
                new LobbyMemberSnapshot(1UL, "Hote", "char_rookie", true)
            }, false, 20, 5);
            roster.Tick();

            Assert.That(raiseCount, Is.EqualTo(1), "seuls les reglages de trafic ont change : l'evenement doit partir");
            Assert.That(roster.Current.AiVehicleTargetCount, Is.EqualTo(20));
            Assert.That(roster.Current.LitterThrowerCount, Is.EqualTo(5));
        }

        [Test]
        public void RosterSnapshotKeepsTheUnresolvedSentinelWhenNothingWasPublished()
        {
            Assert.That(LobbyRosterSnapshot.Empty.AiVehicleTargetCount, Is.EqualTo(SessionTrafficValue.Unresolved));
            Assert.That(LobbyRosterSnapshot.Empty.LitterThrowerCount, Is.EqualTo(SessionTrafficValue.Unresolved));

            var withoutTraffic = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0]);
            Assert.That(withoutTraffic.AiVehicleTargetCount, Is.EqualTo(SessionTrafficValue.Unresolved),
                "un instantane construit sans reglages de trafic reste non resolu : les doubles des Stories 2.2/2.3/2.4/2.8 restent valides");
            Assert.That(withoutTraffic.LitterThrowerCount, Is.EqualTo(SessionTrafficValue.Unresolved));
        }

        // --- Couture LobbyFlowController : bornes, refus, publication --------------------------

        [Test]
        public void AnAcceptedStepUpdatesTheSessionValueAndPublishesIt()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out var lobbyPlatform, out _, openRoom: true);

            InvokeStep(context, "HandleAiVehicleTargetStepRequested", 1);
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(9));
            Assert.That(lobbyPlatform.LastAiVehicleTargetCount, Is.EqualTo(9));

            InvokeStep(context, "HandleLitterThrowerStepRequested", 1);
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(3));
            Assert.That(lobbyPlatform.LastLitterThrowerCount, Is.EqualTo(3));
        }

        [Test]
        public void RefusingToDropTheTargetBelowTheLittererCountKeepsBothValues()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out var lobbyPlatform, out _, openRoom: true);

            InvokeStep(context, "HandleLitterThrowerStepRequested", 1);
            InvokeStep(context, "HandleLitterThrowerStepRequested", 1);
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(4));
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(8));

            var publishCountBefore = lobbyPlatform.SetTrafficSettingsCallCount;
            var litterersBefore = context.Settings.LitterThrowerCount;

            for (var i = 0; i < 5; i++)
            {
                InvokeStep(context, "HandleAiVehicleTargetStepRequested", -1);
            }

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(4),
                "le total s'arrete au nombre de jeteurs : il ne peut pas passer dessous");
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(litterersBefore), "la valeur refusee ne bouge pas");
            Assert.That(lobbyPlatform.SetTrafficSettingsCallCount, Is.GreaterThan(publishCountBefore),
                "les paliers acceptes ont bien ete publies");
        }

        [Test]
        public void RefusingAStepAtTheAuthoredBoundKeepsTheValue()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out var lobbyPlatform, out _, openRoom: true);

            for (var i = 0; i < 40; i++)
            {
                InvokeStep(context, "HandleAiVehicleTargetStepRequested", 1);
            }

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(def.MaxTargetPopulation),
                "la borne authorée haute est infranchissable");

            // Les jeteurs descendent d'abord a 0 : sans cela, l'invariant "jeteurs <= effectif" plancher
            // l'effectif a 2 et la borne basse authorée ne serait jamais atteinte.
            for (var i = 0; i < 40; i++)
            {
                InvokeStep(context, "HandleLitterThrowerStepRequested", -1);
            }

            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(def.MinLitterThrowers));

            for (var i = 0; i < 40; i++)
            {
                InvokeStep(context, "HandleAiVehicleTargetStepRequested", -1);
            }

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(def.MinTargetPopulation),
                "la borne authorée basse est infranchissable");
            Assert.That(lobbyPlatform.SetTrafficSettingsCallCount, Is.GreaterThan(0), "les paliers acceptes ont ete publies");
        }

        [Test]
        public void TheEffectiveLittererUpperBoundIsCappedByTheCurrentTarget()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out _, out _, openRoom: true);

            Assert.That(InvokeInt(context, "EffectiveMaxLitterThrowers"), Is.EqualTo(8),
                "effectif 8 / borne authorée 8 : la borne effective est 8");

            // Les jeteurs descendent d'abord a 0, sinon l'invariant plancher l'effectif a 2.
            for (var i = 0; i < 40; i++)
            {
                InvokeStep(context, "HandleLitterThrowerStepRequested", -1);
            }

            for (var i = 0; i < 40; i++)
            {
                InvokeStep(context, "HandleAiVehicleTargetStepRequested", -1);
            }

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(0));
            Assert.That(InvokeInt(context, "EffectiveMaxLitterThrowers"), Is.EqualTo(0),
                "effectif 0 : la borne effective des jeteurs tombe a 0, l'ecran desactive donc le bouton +");
        }

        [Test]
        public void WithoutAnOpenHostRoomNoStepChangesAnything()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out var lobbyPlatform, out _, openRoom: false);

            var before = context.Settings.AiVehicleTargetCount;
            InvokeStep(context, "HandleAiVehicleTargetStepRequested", 1);
            InvokeStep(context, "HandleLitterThrowerStepRequested", 1);

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(before),
                "un invite n'edite pas les reglages de trafic de l'hote");
            Assert.That(lobbyPlatform.SetTrafficSettingsCallCount, Is.EqualTo(0));
        }

        [Test]
        public void WithoutATrafficDefNoStepChangesAnything()
        {
            var context = BuildFlowController(null, out _, out _, openRoom: true);
            var screen = (LobbyRosterScreen)GetFieldValue(context, "lobbyRosterScreen");

            InvokeStep(context, "HandleAiVehicleTargetStepRequested", 1);
            InvokeStep(context, "HandleLitterThrowerStepRequested", 1);
            InvokeVoid(context, "RefreshTrafficSettingsPresentation");

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(SessionTrafficValue.Unresolved),
                "sans Def il n'existe aucune borne : aucune valeur n'est resolue, et aucune borne n'est inventee");
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(SessionTrafficValue.Unresolved));
            AssertTrafficButtons(screen, false);
        }

        [Test]
        public void AJoinedClientPicksUpTheHostValuesAndNeverOverwritesItsOwnReadWithNothing()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out _, out _, openRoom: false);

            var applyMethod = typeof(LobbyFlowController).GetMethod("ApplyHostTrafficSettings", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(applyMethod, Is.Not.Null, "ApplyHostTrafficSettings attendu sur LobbyFlowController");

            var published = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 17, 4);
            var changed = (bool)applyMethod.Invoke(context, new object[] { published });
            Assert.That(changed, Is.True);
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(17));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(4));

            var unresolved = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0]);
            var changedAgain = (bool)applyMethod.Invoke(context, new object[] { unresolved });
            Assert.That(changedAgain, Is.False, "un instantane non resolu n'ecrase jamais une valeur deja recue");
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(17));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(4));
        }

        [Test]
        public void ReceivedLobbyValuesAreClampedAsAValidPair()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out _, out _, openRoom: false);
            var applyMethod = typeof(LobbyFlowController).GetMethod("ApplyHostTrafficSettings", BindingFlags.Instance | BindingFlags.NonPublic);

            applyMethod.Invoke(context, new object[]
            {
                new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 999, 200)
            });
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(def.MaxTargetPopulation));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(def.MaxLitterThrowers));

            applyMethod.Invoke(context, new object[]
            {
                new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 3, 7)
            });
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(3));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(3), "les jeteurs restent sous l'effectif recu");
        }

        [Test]
        public void ReturningHostRestoresThePersistentSettingsBeforeRepublishingAnEdit()
        {
            var def = BuildDef(8, 2, 0, 8);
            var context = BuildFlowController(def, out var platform, out _, openRoom: true);
            var roster = (LobbyRosterService)GetFieldValue(context, "lobbyRoster");
            platform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 17, 4);
            roster.Tick();

            InvokeVoid(context, "RestoreHostTrafficSettingsFromLobby");
            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(17));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(4));

            InvokeStep(context, "HandleAiVehicleTargetStepRequested", 1);
            Assert.That(platform.LastAiVehicleTargetCount, Is.EqualTo(18));
            Assert.That(platform.LastLitterThrowerCount, Is.EqualTo(4), "une edition conserve l'autre valeur persistante");
        }

        // --- Ecran de roster : butee visible ------------------------------------------------------

        [Test]
        public void TheStepButtonsAreDisabledAtTheirEffectiveBounds()
        {
            var screen = BuildScreen(out var vehiclesDecrease, out var vehiclesIncrease, out var litterDecrease, out var litterIncrease);

            screen.ShowTrafficSettings(8, 0, 30, 2, 0, 8, true);
            Assert.That(vehiclesDecrease.interactable, Is.True);
            Assert.That(vehiclesIncrease.interactable, Is.True);
            Assert.That(litterDecrease.interactable, Is.True);
            Assert.That(litterIncrease.interactable, Is.True);

            screen.ShowTrafficSettings(30, 0, 30, 8, 0, 8, true);
            Assert.That(vehiclesIncrease.interactable, Is.False, "borne haute atteinte : le bouton + est inactif");
            Assert.That(litterIncrease.interactable, Is.False);

            screen.ShowTrafficSettings(0, 0, 30, 0, 0, 8, true);
            Assert.That(vehiclesDecrease.interactable, Is.False, "borne basse atteinte : le bouton - est inactif");
            Assert.That(litterDecrease.interactable, Is.False);

            screen.ShowTrafficSettings(8, 0, 30, 2, 0, 8, false);
            Assert.That(vehiclesIncrease.interactable, Is.False, "edition refusee : tous les pas sont inactifs");
            Assert.That(vehiclesDecrease.interactable, Is.False);
            Assert.That(litterIncrease.interactable, Is.False);
            Assert.That(litterDecrease.interactable, Is.False);
        }

        [Test]
        public void TheStepButtonsRaiseTheirIntentWithTheirOwnDirection()
        {
            var screen = BuildScreen(out _, out var vehiclesIncrease, out var litterDecrease, out _);
            InvokeAwake(screen);

            var vehicleSteps = new System.Collections.Generic.List<int>();
            var litterSteps = new System.Collections.Generic.List<int>();
            screen.AiVehicleTargetStepRequested += vehicleSteps.Add;
            screen.LitterThrowerStepRequested += litterSteps.Add;

            vehiclesIncrease.onClick.Invoke();
            Assert.That(vehicleSteps, Is.EqualTo(new[] { 1 }), "le bouton + des vehicules demande un pas montant");
            Assert.That(litterSteps, Is.Empty, "un bouton de vehicules ne touche jamais aux jeteurs");

            litterDecrease.onClick.Invoke();
            Assert.That(vehicleSteps, Is.EqualTo(new[] { 1 }), "un bouton de jeteurs ne touche jamais aux vehicules");
            Assert.That(litterSteps, Is.EqualTo(new[] { -1 }), "le bouton - des jeteurs demande un pas descendant");
        }

        // --- Gardes de source : ce qu'un test EditMode ne peut pas executer ------------------------

        [Test]
        public void TheSpawnerResolvesTheSessionValueFromTheRunStateAndFallsBackOnlyOnTheAuthoredDef()
        {
            var spawnerSource = CodeOnly(File.ReadAllText(SpawnerSourcePath));

            Assert.That(Occurrences(spawnerSource, "ResolveSessionTargetPopulation"), Is.EqualTo(2),
                "toujours exactement une definition et un appel : le point de branchement reste unique");
            Assert.That(spawnerSource, Does.Contain("runState.AiVehicleTargetCount.Value"),
                "la valeur de session resolue se lit dans NetworkedRunState, seul etat autoritatif de la run");
            Assert.That(spawnerSource, Does.Contain("settings.ClampTargetPopulation(settings.DefaultTargetPopulation)"),
                "le defaut authore du Def reste le SEUL repli quand aucune session n'a resolu la valeur");
            Assert.That(spawnerSource, Does.Contain("PublishSessionTrafficSettingsOnce"),
                "l'hote depose les valeurs resolues une seule fois, depuis l'instantane du lobby persistant");
            Assert.That(spawnerSource, Does.Contain("ClampLitterThrowers"),
                "le nombre de jeteurs est resolu dans le meme mouvement que l'effectif");

            var sceneText = File.ReadAllText(MvpRunScenePath);
            Assert.That(sceneText, Does.Not.Contain("TargetPopulation"),
                "aucun effectif n'est authore dans la scene : il vit uniquement dans le Def");
        }

        /// <summary>
        /// Le branchement de la Story 5.16 ajoute du code dans la boucle d'insertion : cet ajout ne doit
        /// ouvrir aucun second chemin de retrait. La garde equivalente de la Story 5.10
        /// (NothingRemovesATrafficVehicleAnywhereButAnExitPortal) est aujourd'hui rouge pour une raison
        /// sans rapport -- elle interdit la sous-chaine "Despawn(", que le nom OnNetworkDespawn contient
        /// deja -- et elle avorte donc AVANT d'evaluer ses assertions sur le spawner. Ces proprietes sont
        /// donc reprises ici, sur le fichier reellement modifie par cette story.
        /// </summary>
        [Test]
        public void AddingTheSessionBranchOpensNoSecondRemovalPathInTheSpawner()
        {
            var spawnerSource = File.ReadAllText(SpawnerSourcePath);

            Assert.That(Occurrences(CodeOnly(spawnerSource), "Despawn("), Is.EqualTo(1),
                "un seul chemin de despawn dans tout le trafic, avant comme apres la Story 5.16");

            foreach (var forbidden in new[] { "distanceToPlayer", "despawnRadius", "maxLifetime", "IsRolledOver", "voidHeight" })
            {
                Assert.That(spawnerSource, Does.Not.Contain(forbidden),
                    "aucun motif de retrait autre que le portail de sortie (AD-34) : '" + forbidden + "' n'a rien a faire ici");
            }
        }

        [Test]
        public void TheFlowControllerOwnsBoundsRefusalAndPublicationInOnePlace()
        {
            var source = CodeOnly(File.ReadAllText(FlowControllerSourcePath));

            Assert.That(source, Does.Contain("PublishUnavailable(TrafficLitterersExceedVehiclesMessage)"),
                "le seul refus reellement atteignable publie une notice distincte et visible");
            Assert.That(Occurrences(source, "lobbyRoster.PublishTrafficSettings("), Is.EqualTo(2),
                "une publication a l'ouverture de la room, une a chaque valeur acceptee : aucun autre chemin");
            Assert.That(source, Does.Contain("if (isJoinedClient && snapshot.HasLobby && ApplyHostTrafficSettings(snapshot))"),
                "l'hote ne se reapplique jamais sa propre lecture : seul un invite reprend les valeurs publiees");
            Assert.That(source, Does.Not.Contain("SetLobbyTrafficSettings("),
                "l'App ne parle jamais directement a la plateforme Steam : elle passe par LobbyRosterService");
        }

        [Test]
        public void TheLobbySceneCarriesTheTrafficRowsAndTheAuthoredDef()
        {
            var sceneText = File.ReadAllText(MainMenuLobbyScenePath);

            // Une reference non assignee s'ecrit {fileID: 0} et satisferait une simple recherche de nom :
            // ces six references doivent pointer sur un objet reel, sinon un bouton meurt en silence
            // avec pour seule trace un avertissement d'Awake.
            foreach (var field in new[]
            {
                "trafficVehiclesLabel",
                "trafficVehiclesDecreaseButton",
                "trafficVehiclesIncreaseButton",
                "litterThrowersLabel",
                "litterThrowersDecreaseButton",
                "litterThrowersIncreaseButton"
            })
            {
                Assert.That(Regex.IsMatch(sceneText, field + @": \{fileID: (?!0\b)\d+\}"), Is.True,
                    field + " doit pointer sur un objet reel dans MainMenuLobby, jamais sur {fileID: 0}");
            }

            var defGuid = Regex.Match(
                File.ReadAllText(TrafficSettingsDefAssetPath + ".meta"), @"guid: ([0-9a-f]{32})").Groups[1].Value;
            Assert.That(defGuid, Is.Not.Empty, "le .meta du Def doit porter un guid");
            Assert.That(sceneText, Does.Contain("trafficSettings: {fileID: 11400000, guid: " + defGuid + ", type: 2}"),
                "le TrafficSettingsDef_Default est assigne sur LobbyFlowController : un autre Def, ou aucun, laisserait les reglages inertes");
        }

        [Test]
        public void TheLobbyReadsEveryBoundAndDefaultFromTheAuthoredDefAndTheScreenKnowsNoneOfIt()
        {
            var flowSource = CodeOnly(File.ReadAllText(FlowControllerSourcePath));
            foreach (var reference in new[]
            {
                "trafficSettings.DefaultTargetPopulation",
                "trafficSettings.DefaultLitterThrowers",
                "trafficSettings.MinTargetPopulation",
                "trafficSettings.MaxTargetPopulation",
                "trafficSettings.MinLitterThrowers",
                "trafficSettings.MaxLitterThrowers"
            })
            {
                Assert.That(flowSource, Does.Contain(reference),
                    reference + " : chaque borne et chaque defaut vient du Def authoré, jamais d'une constante de code");
            }

            var screenSource = CodeOnly(File.ReadAllText("Assets/RoadRage/Features/UI/LobbyRosterScreen.cs"));
            Assert.That(screenSource, Does.Not.Contain("TrafficSettingsDef"),
                "l'ecran ne connait que des entiers et des bornes entieres : ni le Def, ni MatchSettings");
            Assert.That(screenSource, Does.Not.Contain("MatchSettings"),
                "l'ecran ne mute jamais les reglages de partie lui-meme (garde de la Story 1.2)");
        }

        // --- Resolution cote run, contrat de plateforme, amorcage, affichage -------------------------

        [Test]
        public void TheResolvedPopulationComesFromTheSessionValueAndNotFromTheAuthoredDefault()
        {
            var def = BuildDef(8, 2, 0, 8);
            var runState = BuildRunState();
            var spawner = BuildSpawner(runState);

            runState.AiVehicleTargetCount.Value = 17;
            Assert.That(InvokeResolve(spawner, def), Is.EqualTo(17),
                "une valeur de session differente du defaut authore (8) doit l'emporter : c'est l'apport central de la story");

            runState.AiVehicleTargetCount.Value = 999;
            Assert.That(InvokeResolve(spawner, def), Is.EqualTo(def.MaxTargetPopulation),
                "la valeur de session reste bornee par le Def, jamais prise telle quelle");

            runState.AiVehicleTargetCount.Value = SessionTrafficValue.Unresolved;
            Assert.That(InvokeResolve(spawner, def), Is.EqualTo(def.DefaultTargetPopulation),
                "sans session resolue, le defaut authore du Def est la seule source");
        }

        [Test]
        public void SpawnerWaitsForTheLobbySnapshotThenPublishesTheResolvedPair()
        {
            var onlineServices = BuildOnlineServices();
            var platform = new FakeSteamLobbyPlatform
            {
                NextCreateOutcome = new LobbyCreateOutcome(true, 1UL),
                NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0])
            };
            var room = new LobbyRoomService(platform, onlineServices);
            room.CreateRoomAsync().GetAwaiter().GetResult();
            var join = new LobbyJoinService(platform, onlineServices);
            var roster = new LobbyRosterService(platform, room, join);
            roster.Tick();

            var instanceField = typeof(RoadRageBootstrap).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic);
            previousBootstrap = (RoadRageBootstrap)instanceField.GetValue(null);
            var bootstrapObject = new GameObject("Story516BootstrapUnderTest");
            bootstrapObject.SetActive(false);
            testBootstrap = bootstrapObject.AddComponent<RoadRageBootstrap>();
            instanceField.SetValue(null, testBootstrap);
            SetField(testBootstrap, "LobbyRoom", room);
            SetField(testBootstrap, "LobbyRoster", roster);
            SetField(testBootstrap, "OnlineServices", onlineServices);

            var def = BuildDef(8, 2, 0, 8);
            var runState = BuildRunState();
            var spawner = BuildSpawner(runState);
            var publish = typeof(PortalTrafficSpawner).GetMethod("PublishSessionTrafficSettingsOnce", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(publish, Is.Not.Null);

            Assert.That((bool)publish.Invoke(spawner, new object[] { def }), Is.False,
                "une room ouverte sans instantane resolu suspend le spawn au lieu d'utiliser le defaut");
            Assert.That(runState.AiVehicleTargetCount.Value, Is.EqualTo(SessionTrafficValue.Unresolved));

            platform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 17, 4);
            roster.Tick();
            Assert.That((bool)publish.Invoke(spawner, new object[] { def }), Is.True);
            Assert.That(runState.AiVehicleTargetCount.Value, Is.EqualTo(17));
            Assert.That(runState.LitterThrowerCount.Value, Is.EqualTo(4));

            var inconsistentSpawner = BuildSpawner(runState);
            platform.NextRoster = new LobbyRosterSnapshot(true, 1UL, Difficulty.Normal, new LobbyMemberSnapshot[0], false, 3, 7);
            roster.Tick();
            Assert.That((bool)publish.Invoke(inconsistentSpawner, new object[] { def }), Is.True);
            Assert.That(runState.AiVehicleTargetCount.Value, Is.EqualTo(3));
            Assert.That(runState.LitterThrowerCount.Value, Is.EqualTo(3), "la paire conserve l'invariant au transfert");

            var source = CodeOnly(File.ReadAllText(SpawnerSourcePath));
            Assert.That(source, Does.Contain("if (!PublishSessionTrafficSettingsOnce(settings))"),
                "FixedUpdate ne compose pas de population pendant que la publication du lobby est en attente");
        }

        [Test]
        public void TheRealSteamPlatformDeclaresTheTrafficPublisherItself()
        {
            var method = typeof(FacepunchSteamLobbyPlatform).GetMethod("SetLobbyTrafficSettings");

            Assert.That(method, Is.Not.Null,
                "FacepunchSteamLobbyPlatform doit declarer l'override : le corps par defaut vide de l'interface rendrait sa disparition silencieuse");
            Assert.That(method.DeclaringType, Is.EqualTo(typeof(FacepunchSteamLobbyPlatform)),
                "l'override doit venir de l'implementation reelle, pas du corps par defaut de l'interface");
        }

        [Test]
        public void SteamTrafficMetadataUsesMatchingKeysAndRoundTripsInvariantIntegers()
        {
            var platformType = typeof(FacepunchSteamLobbyPlatform);
            var aiKey = platformType.GetField("AiVehicleTargetCountDataKey", BindingFlags.Static | BindingFlags.NonPublic);
            var litterKey = platformType.GetField("LitterThrowerCountDataKey", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(aiKey.GetRawConstantValue(), Is.EqualTo("aiVehicleTargetCount"));
            Assert.That(litterKey.GetRawConstantValue(), Is.EqualTo("litterThrowerCount"));

            var format = platformType.GetMethod("FormatTrafficValue", BindingFlags.Static | BindingFlags.NonPublic);
            var parse = platformType.GetMethod("ParseTrafficValue", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(format, Is.Not.Null);
            Assert.That(parse, Is.Not.Null);
            foreach (var value in new[] { 0, 17, 30 })
            {
                var encoded = (string)format.Invoke(null, new object[] { value });
                Assert.That((int)parse.Invoke(null, new object[] { encoded }), Is.EqualTo(value));
            }

            Assert.That((int)parse.Invoke(null, new object[] { "not-an-integer" }), Is.EqualTo(SessionTrafficValue.Unresolved));
            Assert.That((int)parse.Invoke(null, new object[] { "-2" }), Is.EqualTo(SessionTrafficValue.Unresolved));

            var source = CodeOnly(File.ReadAllText("Assets/RoadRage/Features/Online/FacepunchSteamLobbyPlatform.cs"));
            Assert.That(Occurrences(source, "AiVehicleTargetCountDataKey"), Is.EqualTo(3), "la meme cle sert a l'ecriture et a la lecture");
            Assert.That(Occurrences(source, "LitterThrowerCountDataKey"), Is.EqualTo(3), "la meme cle sert a l'ecriture et a la lecture");
        }

        [Test]
        public void TheLobbySeedsBothValuesFromTheDef()
        {
            var def = BuildDef(3, 3, 0, 8);
            var context = BuildFlowController(def, out _, out _, openRoom: false, seedSettings: false);

            InvokeVoid(context, "SeedTrafficSettingsFromDef");

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(3), "l'effectif de depart vient du Def authoré");
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(3),
                "le defaut de jeteurs est plafonne par l'effectif cible : l'invariant tient des l'amorçage");
        }

        [Test]
        public void TheLobbyWithoutADefLeavesTheValuesUnresolvedAndWarns()
        {
            var context = BuildFlowController(null, out _, out _, openRoom: false, seedSettings: false);

            LogAssert.Expect(LogType.Warning, new Regex("sans TrafficSettingsDef"));
            InvokeVoid(context, "SeedTrafficSettingsFromDef");

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(SessionTrafficValue.Unresolved));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(SessionTrafficValue.Unresolved));
        }

        [Test]
        public void TheLobbySubscribesBothStepEventsAndSeedsInAwake()
        {
            var source = CodeOnly(File.ReadAllText(FlowControllerSourcePath));

            foreach (var wiring in new[]
            {
                "lobbyRosterScreen.AiVehicleTargetStepRequested += HandleAiVehicleTargetStepRequested;",
                "lobbyRosterScreen.LitterThrowerStepRequested += HandleLitterThrowerStepRequested;",
                "lobbyRosterScreen.AiVehicleTargetStepRequested -= HandleAiVehicleTargetStepRequested;",
                "lobbyRosterScreen.LitterThrowerStepRequested -= HandleLitterThrowerStepRequested;",
                "SeedTrafficSettingsFromDef();",
                "RestoreHostTrafficSettingsFromLobby();"
            })
            {
                Assert.That(source, Does.Contain(wiring),
                    wiring + " : sans ce cablage, une ligne de reglage est morte en jeu sans qu'aucun test ne rougisse");
            }
        }

        [Test]
        public void TheDisplayedLittererBoundIsTheEffectiveOneNotTheAuthoredOne()
        {
            var def = BuildDef(3, 3, 0, 8);
            var context = BuildFlowController(def, out _, out _, openRoom: true);
            var screen = BuildScreen(out _, out _, out _, out var litterIncrease);
            SetField(context, "lobbyRosterScreen", screen);

            InvokeVoid(context, "RefreshTrafficSettingsPresentation");

            Assert.That(context.Settings.AiVehicleTargetCount, Is.EqualTo(3));
            Assert.That(context.Settings.LitterThrowerCount, Is.EqualTo(3));
            Assert.That(litterIncrease.interactable, Is.False,
                "borne haute effective = min(borne authorée 8, effectif 3) = 3 : le bouton est en butee ; avec la borne authorée seule (8) il resterait actif et l'ecran annoncerait une limite que le code n'applique pas");
        }

        // --- Fabriques -----------------------------------------------------------------------------

        private static TrafficSettingsDef BuildDef(int defaultTargetPopulation, int defaultLitterThrowers, int minLitterThrowers, int maxLitterThrowers)
        {
            var def = ScriptableObject.CreateInstance<TrafficSettingsDef>();
            var serialized = new SerializedObject(def);
            serialized.FindProperty("id").stringValue = "traffic_test";
            serialized.FindProperty("defaultTargetPopulation").intValue = defaultTargetPopulation;
            serialized.FindProperty("minTargetPopulation").intValue = 0;
            serialized.FindProperty("maxTargetPopulation").intValue = 30;
            serialized.FindProperty("defaultLitterThrowers").intValue = defaultLitterThrowers;
            serialized.FindProperty("minLitterThrowers").intValue = minLitterThrowers;
            serialized.FindProperty("maxLitterThrowers").intValue = maxLitterThrowers;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return def;
        }

        private static LobbyRosterService BuildService(out FakeSteamLobbyPlatform lobbyPlatform, out LobbyRoomService lobbyRoom, out LobbyJoinService lobbyJoin)
        {
            var onlineServices = BuildOnlineServices();
            lobbyPlatform = new FakeSteamLobbyPlatform();
            lobbyRoom = new LobbyRoomService(lobbyPlatform, onlineServices);
            lobbyJoin = new LobbyJoinService(lobbyPlatform, onlineServices);
            return new LobbyRosterService(lobbyPlatform, lobbyRoom, lobbyJoin);
        }

        /// <summary>
        /// Monte un LobbyFlowController reel sur un GameObject INACTIF : Awake n'a donc pas tourne, et
        /// les dependances sont injectees. Les champs observes sont ensuite ceux du vrai code de
        /// decision -- bornes, refus, publication -- pas une reimplementation du test.
        /// </summary>
        private static LobbyFlowController BuildFlowController(TrafficSettingsDef def, out FakeSteamLobbyPlatform lobbyPlatform, out LobbyRoomService lobbyRoom, bool openRoom, bool seedSettings = true)
        {
            var onlineServices = BuildOnlineServices();
            lobbyPlatform = new FakeSteamLobbyPlatform();
            lobbyRoom = new LobbyRoomService(lobbyPlatform, onlineServices);
            var lobbyJoin = new LobbyJoinService(lobbyPlatform, onlineServices);
            var roster = new LobbyRosterService(lobbyPlatform, lobbyRoom, lobbyJoin);

            var host = new GameObject("Story516FlowControllerUnderTest");
            host.SetActive(false);
            var controller = host.AddComponent<LobbyFlowController>();
            var screenHost = new GameObject("Story516RosterScreenUnderTest");
            screenHost.SetActive(false);
            var rosterScreen = screenHost.AddComponent<LobbyRosterScreen>();
            SetField(rosterScreen, "trafficVehiclesDecreaseButton", BuildButton("FlowVehiclesDecrease"));
            SetField(rosterScreen, "trafficVehiclesIncreaseButton", BuildButton("FlowVehiclesIncrease"));
            SetField(rosterScreen, "litterThrowersDecreaseButton", BuildButton("FlowLitterDecrease"));
            SetField(rosterScreen, "litterThrowersIncreaseButton", BuildButton("FlowLitterIncrease"));

            if (openRoom)
            {
                lobbyPlatform.NextCreateOutcome = new LobbyCreateOutcome(true, 1UL);
                lobbyRoom.CreateRoomAsync().GetAwaiter().GetResult();
            }

            var settings = new MatchSettings();
            if (def != null && seedSettings)
            {
                settings.AiVehicleTargetCount = def.DefaultTargetPopulation;
                settings.LitterThrowerCount = Mathf.Min(def.DefaultLitterThrowers, settings.AiVehicleTargetCount);
            }

            SetField(controller, "Settings", settings);
            SetField(controller, "trafficSettings", def);
            SetField(controller, "lobbyRoom", lobbyRoom);
            SetField(controller, "lobbyRoster", roster);
            SetField(controller, "lobbyRosterScreen", rosterScreen);
            return controller;
        }

        /// <summary>Ecran reel monte a froid : seuls les boutons sont injectes, aucun libelle (les gardes de nullite suffisent).</summary>
        private static LobbyRosterScreen BuildScreen(out Button vehiclesDecrease, out Button vehiclesIncrease, out Button litterDecrease, out Button litterIncrease)
        {
            var host = new GameObject("Story516RosterScreenStepButtons");
            host.SetActive(false);
            var screen = host.AddComponent<LobbyRosterScreen>();

            vehiclesDecrease = BuildButton("vehiclesDecrease");
            vehiclesIncrease = BuildButton("vehiclesIncrease");
            litterDecrease = BuildButton("litterDecrease");
            litterIncrease = BuildButton("litterIncrease");

            SetField(screen, "trafficVehiclesDecreaseButton", vehiclesDecrease);
            SetField(screen, "trafficVehiclesIncreaseButton", vehiclesIncrease);
            SetField(screen, "litterThrowersDecreaseButton", litterDecrease);
            SetField(screen, "litterThrowersIncreaseButton", litterIncrease);
            return screen;
        }

        private static Button BuildButton(string name)
        {
            var host = new GameObject("Story516" + name);
            host.SetActive(false);
            return host.AddComponent<Button>();
        }

        private static OnlineServicesBootstrapService BuildOnlineServices()
        {
            var steamPlatform = new FakeSteamPlatform { IsValid = true, IsLoggedOn = true };
            var onlineServices = new OnlineServicesBootstrapService(steamPlatform, TestAppId);
            onlineServices.TryInitialize();
            Assert.That(onlineServices.Status, Is.EqualTo(OnlineServicesStatus.Online));
            return onlineServices;
        }

        /// <summary>Ecarte les commentaires et les chaines de documentation : les gardes portent sur du code, pas sur de la prose.</summary>
        private static string CodeOnly(string source)
        {
            var withoutBlockComments = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var withoutLineComments = Regex.Replace(withoutBlockComments, @"//.*?$", string.Empty, RegexOptions.Multiline);
            return Regex.Replace(withoutLineComments, @"///.*?$", string.Empty, RegexOptions.Multiline);
        }

        private static int Occurrences(string source, string token)
        {
            var count = 0;
            var index = source.IndexOf(token, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = source.IndexOf(token, index + token.Length, StringComparison.Ordinal);
            }

            return count;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                field = target.GetType().GetField("<" + fieldName + ">k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            Assert.That(field, Is.Not.Null, fieldName + " attendu sur " + target.GetType().Name);
            field.SetValue(target, value);
        }

        private static object GetFieldValue(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName + " attendu sur " + target.GetType().Name);
            return field.GetValue(target);
        }

        private static void AssertTrafficButtons(LobbyRosterScreen screen, bool interactable)
        {
            foreach (var fieldName in new[]
            {
                "trafficVehiclesDecreaseButton",
                "trafficVehiclesIncreaseButton",
                "litterThrowersDecreaseButton",
                "litterThrowersIncreaseButton"
            })
            {
                var button = (Button)GetFieldValue(screen, fieldName);
                Assert.That(button.interactable, Is.EqualTo(interactable), fieldName);
            }
        }

        private static void InvokeStep(LobbyFlowController controller, string methodName, int step)
        {
            var method = typeof(LobbyFlowController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName + " attendu sur LobbyFlowController");
            method.Invoke(controller, new object[] { step });
        }

        private static int InvokeInt(LobbyFlowController controller, string methodName)
        {
            var method = typeof(LobbyFlowController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName + " attendu sur LobbyFlowController");
            return (int)method.Invoke(controller, null);
        }

        private static void InvokeVoid(LobbyFlowController controller, string methodName)
        {
            var method = typeof(LobbyFlowController).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName + " attendu sur LobbyFlowController");
            method.Invoke(controller, null);
        }

        /// <summary>Etat de run reel, monte comme le fait la suite Story 5.6 : AddComponent, jamais .Spawn().</summary>
        private static NetworkedRunState BuildRunState()
        {
            var host = new GameObject("Story516RunStateUnderTest");
            host.AddComponent<NetworkObject>();
            return host.AddComponent<NetworkedRunState>();
        }

        private static PortalTrafficSpawner BuildSpawner(NetworkedRunState runState)
        {
            var host = new GameObject("Story516SpawnerUnderTest");
            host.SetActive(false);
            var spawner = host.AddComponent<PortalTrafficSpawner>();
            SetField(spawner, "runState", runState);
            return spawner;
        }

        private static int InvokeResolve(PortalTrafficSpawner spawner, TrafficSettingsDef def)
        {
            var method = typeof(PortalTrafficSpawner).GetMethod("ResolveSessionTargetPopulation", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "ResolveSessionTargetPopulation attendu sur PortalTrafficSpawner");
            return (int)method.Invoke(spawner, new object[] { def });
        }

        /// <summary>
        /// Execute l'Awake d'un composant monte a froid, ce que Unity ne fait pas sur un GameObject
        /// inactif. C'est ce qui cable les boutons de pas a leurs intentions.
        /// </summary>
        private static void InvokeAwake(MonoBehaviour target)
        {
            var awake = target.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(awake, Is.Not.Null, "Awake attendu sur " + target.GetType().Name);
            awake.Invoke(target, null);
        }

        private sealed class FakeSteamPlatform : ISteamPlatform
        {
            public bool IsValid { get; set; }

            public bool IsLoggedOn { get; set; }

            public void Init(uint appId)
            {
            }

            public void Shutdown()
            {
            }

            public void RunCallbacks()
            {
            }
        }

        private sealed class FakeSteamLobbyPlatform : ISteamLobbyPlatform
        {
            public LobbyCreateOutcome NextCreateOutcome { get; set; } = LobbyCreateOutcome.Failed;

            public LobbyJoinOutcome NextJoinOutcome { get; set; } = LobbyJoinOutcome.Failed;

            public LobbyRosterSnapshot NextRoster { get; set; } = LobbyRosterSnapshot.Empty;

            public int SetTrafficSettingsCallCount { get; private set; }

            public int LastAiVehicleTargetCount { get; private set; }

            public int LastLitterThrowerCount { get; private set; }

            public Task<LobbyCreateOutcome> CreateLobbyAsync(int maxMembers)
            {
                return Task.FromResult(NextCreateOutcome);
            }

            public Task<LobbyJoinOutcome> JoinLobbyAsync(ulong lobbyId)
            {
                return Task.FromResult(NextJoinOutcome);
            }

            public void LeaveCurrentLobby()
            {
            }

            public LobbyRosterSnapshot GetRosterSnapshot()
            {
                return NextRoster;
            }

            public void SetLocalMemberReady(bool ready)
            {
            }

            public void SetLocalMemberProfile(string displayName, string characterId)
            {
            }

            public void SetLobbyDifficulty(Difficulty difficulty)
            {
            }

            public void SetLobbyTrafficSettings(int aiVehicleTargetCount, int litterThrowerCount)
            {
                SetTrafficSettingsCallCount++;
                LastAiVehicleTargetCount = aiVehicleTargetCount;
                LastLitterThrowerCount = litterThrowerCount;
            }

            public void SetLobbyRunLaunchRequested(bool launchRequested)
            {
            }
        }
    }
}
