using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoadRage.Features.Vehicles.Traffic
{
    /// <summary>
    /// Code de motif stable d'un echec de validation. Stable signifie : nommable par un test et par
    /// un rapport de migration, jamais renumerote pour des raisons de confort.
    /// </summary>
    public enum RoadModelValidationCode
    {
        EmptyId = 1,
        DuplicateId = 2,
        UnresolvedReference = 3,
        CrossVersionReference = 4,
        IncompatibleManifestRemap = 5,
        TombstonedIdReused = 6,
        NonFiniteNumericValue = 7,
        MovementParentInvalid = 8,
        MovementControlCoverageInvalid = 9,
        SignalizedControlWithoutPlan = 10,
        ConflictingMovementsGreenTogether = 11,
        InvalidGeometryPayload = 12,
        ConflictZoneMembershipInvalid = 13,
        NumericValueOutOfRange = 14,

        // ---------------------------------------------------------------- ordre transversal (AD-48)
        // Quatre codes distincts et non groupes : un rapport de migration (5.27) doit pouvoir
        // nommer le defaut exact, et les separer plus tard casserait la stabilite promise.
        DuplicateLateralOrder = 15,
        NonContiguousLateralOrder = 16,
        MissingCrossSectionDatum = 17,
        MultipleCrossSectionData = 18,

        // ---------------------------------------------------------------- geometrie (Story 5.26)
        // Un code par defaut, jamais groupe. N'est evaluee que sur une source structurellement
        // valide (references resolues) : voir RoadGeometryValidator.

        /// <summary>Tangente ou road-up non unitaire, ou non orthogonaux.</summary>
        NonOrthonormalFrame = 19,

        /// <summary>Depart hors de 0, longueur declaree contre derniere abscisse, ou pas d'abscisse contre corde.</summary>
        InconsistentCurveLength = 20,

        /// <summary>Demi-largeur gauche ou droite nulle ou negative.</summary>
        NonPositiveHalfWidth = 21,

        /// <summary>Intervalle d'adjacence ou abscisse de portail hors du domaine du corridor, ou intervalle vide.</summary>
        ArcPositionOutOfDomain = 22,

        /// <summary>Demi-dimension de frontiere de carrefour ou de volume de conflit non strictement positive.</summary>
        NonPositiveBoxExtents = 23,

        /// <summary>Couture de LaneConnection rompue : position ou tangente.</summary>
        ConnectionSeamBroken = 24,

        /// <summary>Couture d'extremite de JunctionMovement rompue : position, tangente ou largeurs.</summary>
        MovementSeamBroken = 25,

        /// <summary>LaneAdjacency entre deux corridors de sens oppose (AD-47).</summary>
        OppositeDirectionAdjacency = 26,

        /// <summary>LaneAdjacency.Side contredit la geometrie ou l'ordre transversal (AD-48).</summary>
        LaneSideDisagreement = 27,

        /// <summary>Lignes centrales non strictement monotones en LateralOrder le long du datum (AD-48).</summary>
        NonMonotoneLateralOrder = 28,

        /// <summary>Enveloppes transversales voisines qui se recouvrent au-dela de la tolerance (AD-48).</summary>
        OverlappingLateralEnvelopes = 29,

        /// <summary>Corridor sans recouvrement avec le datum, ou ni parallele ni antiparallele a lui (AD-48).</summary>
        CorridorNotGroundedOnDatum = 30,

        // ---------------------------------------------------------------- profil (nettoyage 5.26)

        /// <summary>
        /// Le profil relache un plafond approuve du contrat Road World Model (couture 0,05 m,
        /// tangente 5 deg, longueur/corde 0,05 m). Un profil peut etre PLUS strict ; relacher le
        /// contrat est une revision d'architecture, jamais une valeur de profil.
        /// </summary>
        ProfileToleranceAboveApprovedCeiling = 31,

        // ---------------------------------------------------------------- repere, suite revue 5.26
        // Le code 19 ne porte que la FORME du repere (unitaire, orthogonal) ; les deux defauts
        // d'ORIENTATION ci-dessous ont leur propre code : ils demandent un autre correctif, et un
        // rapport qui groupe par code ne doit pas les confondre avec une erreur de normalisation.

        /// <summary>Road-up retourne (dote negativement au monde-haut) : echange gauche et droite en silence.</summary>
        RoadUpFlipped = 32,

        /// <summary>Tangente opposee ou perpendiculaire a sa corde : le sens de marche ne suit plus la geometrie.</summary>
        TangentOpposesChord = 33
    }

    /// <summary>Un echec de validation : son code stable, l'identifiant fautif et un message.</summary>
    public struct RoadModelValidationIssue
    {
        public RoadModelValidationCode Code;
        public RoadId SubjectId;
        public string Message;

        public RoadModelValidationIssue(RoadModelValidationCode code, RoadId subjectId, string message)
        {
            Code = code;
            SubjectId = subjectId;
            Message = message;
        }

        public override string ToString()
        {
            return Code + " [" + SubjectId + "] " + Message;
        }
    }

    /// <summary>
    /// Echec dur de compilation. Leve par <see cref="RoadModelCompiler.Compile"/> : aucune sortie
    /// compilee n'est emise, aucune version n'est produite.
    /// </summary>
    public sealed class RoadModelCompilationException : Exception
    {
        public RoadModelCompilationException(IReadOnlyList<RoadModelValidationIssue> issues)
            : base(Describe(issues))
        {
            Issues = issues ?? (IReadOnlyList<RoadModelValidationIssue>)new RoadModelValidationIssue[0];
        }

        public IReadOnlyList<RoadModelValidationIssue> Issues { get; private set; }

        public bool HasCode(RoadModelValidationCode code)
        {
            for (int i = 0; i < Issues.Count; i++)
            {
                if (Issues[i].Code == code)
                {
                    return true;
                }
            }

            return false;
        }

        private static string Describe(IReadOnlyList<RoadModelValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                return "Road World Model invalide.";
            }

            var builder = new System.Text.StringBuilder("Road World Model invalide (");
            builder.Append(issues.Count).Append(" echec(s)) :");
            for (int i = 0; i < issues.Count; i++)
            {
                builder.Append("\n  - ").Append(issues[i].ToString());
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Validation structurelle a echec dur du <see cref="RoadModelSource"/> recu, et de lui seul :
    /// aucun import precedent, aucun etat sur disque, aucune reconciliation de lignee (Story 5.27).
    /// </summary>
    public static class RoadModelValidator
    {
        /// <summary>
        /// Retourne la liste des echecs. Vide signifie que la source est compilable.
        /// </summary>
        public static IReadOnlyList<RoadModelValidationIssue> Validate(RoadModelSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var issues = new List<RoadModelValidationIssue>();

            if (source.ModelId.IsEmpty)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, RoadId.None, "Le modele n'a pas d'identifiant."));
            }

            var sections = Safe(source.Sections);
            var corridors = Safe(source.Corridors);
            var connections = Safe(source.Connections);
            var adjacencies = Safe(source.Adjacencies);
            var junctions = Safe(source.Junctions);
            var movements = Safe(source.Movements);
            var controls = Safe(source.Controls);
            var conflictZones = Safe(source.ConflictZones);
            var signalPlans = Safe(source.SignalPlans);
            var portals = Safe(source.Portals);

            // ------------------------------------------------ identite : vide, duplique, portee
            var kindById = new Dictionary<RoadId, RoadRecordKind>();
            for (int i = 0; i < sections.Length; i++)
            {
                RegisterId(issues, kindById, sections[i].Id, RoadRecordKind.Section, "RoadSection");
            }

            for (int i = 0; i < corridors.Length; i++)
            {
                RegisterId(issues, kindById, corridors[i].Id, RoadRecordKind.Corridor, "LaneCorridor");
            }

            for (int i = 0; i < connections.Length; i++)
            {
                RegisterId(issues, kindById, connections[i].Id, RoadRecordKind.Connection, "LaneConnection");
            }

            for (int i = 0; i < adjacencies.Length; i++)
            {
                RegisterId(issues, kindById, adjacencies[i].Id, RoadRecordKind.Adjacency, "LaneAdjacency");
            }

            for (int i = 0; i < junctions.Length; i++)
            {
                RegisterId(issues, kindById, junctions[i].Id, RoadRecordKind.Junction, "Junction");
            }

            for (int i = 0; i < movements.Length; i++)
            {
                RegisterId(issues, kindById, movements[i].Id, RoadRecordKind.Movement, "JunctionMovement");
            }

            for (int i = 0; i < controls.Length; i++)
            {
                RegisterId(issues, kindById, controls[i].Id, RoadRecordKind.Control, "JunctionControl");
            }

            for (int i = 0; i < conflictZones.Length; i++)
            {
                RegisterId(issues, kindById, conflictZones[i].Id, RoadRecordKind.ConflictZone, "ConflictZone");
            }

            for (int i = 0; i < signalPlans.Length; i++)
            {
                RegisterId(issues, kindById, signalPlans[i].Id, RoadRecordKind.SignalPlan, "SignalPlan");
            }

            for (int i = 0; i < portals.Length; i++)
            {
                RegisterId(issues, kindById, portals[i].Id, RoadRecordKind.Portal, "Portal");
            }

            // ------------------------------------------------ valeurs numeriques finies
            CheckFinite(issues, source.ValidationProfile.MaxVehicleHalfWidthMeters, source.ModelId, "ValidationProfile.MaxVehicleHalfWidthMeters");
            CheckFinite(issues, source.ValidationProfile.MaxVehicleLengthMeters, source.ModelId, "ValidationProfile.MaxVehicleLengthMeters");
            CheckFinite(issues, source.ValidationProfile.LateralClearanceMarginMeters, source.ModelId, "ValidationProfile.LateralClearanceMarginMeters");
            CheckFinite(issues, source.ValidationProfile.SeamGapToleranceMeters, source.ModelId, "ValidationProfile.SeamGapToleranceMeters");
            CheckFinite(issues, source.ValidationProfile.SeamTangentToleranceDegrees, source.ModelId, "ValidationProfile.SeamTangentToleranceDegrees");
            CheckFinite(issues, source.ValidationProfile.LengthToleranceMeters, source.ModelId, "ValidationProfile.LengthToleranceMeters");
            CheckFinite(issues, source.ValidationProfile.EnvelopeOverlapToleranceMeters, source.ModelId, "ValidationProfile.EnvelopeOverlapToleranceMeters");
            CheckFinite(issues, source.LocalizationProfile.ScoreBandMeters, source.ModelId, "LocalizationProfile.ScoreBandMeters");
            CheckFinite(issues, source.LocalizationProfile.HysteresisMeters, source.ModelId, "LocalizationProfile.HysteresisMeters");
            CheckFinite(issues, source.LocalizationProfile.AcceptanceDistanceMeters, source.ModelId, "LocalizationProfile.AcceptanceDistanceMeters");
            CheckFinite(issues, source.LocalizationProfile.WrongWayHeadingDegrees, source.ModelId, "LocalizationProfile.WrongWayHeadingDegrees");

            // Domaine des profils (5.26) : une tolerance ou un seuil nul (profil non renseigne) ne
            // doit jamais compiler en silence.
            CheckPositive(issues, source.ValidationProfile.SeamGapToleranceMeters, source.ModelId, "ValidationProfile.SeamGapToleranceMeters");
            CheckPositive(issues, source.ValidationProfile.SeamTangentToleranceDegrees, source.ModelId, "ValidationProfile.SeamTangentToleranceDegrees");
            CheckPositive(issues, source.ValidationProfile.LengthToleranceMeters, source.ModelId, "ValidationProfile.LengthToleranceMeters");
            CheckPositive(issues, source.ValidationProfile.EnvelopeOverlapToleranceMeters, source.ModelId, "ValidationProfile.EnvelopeOverlapToleranceMeters");
            CheckPositive(issues, source.LocalizationProfile.ScoreBandMeters, source.ModelId, "LocalizationProfile.ScoreBandMeters");
            CheckPositive(issues, source.LocalizationProfile.HysteresisMeters, source.ModelId, "LocalizationProfile.HysteresisMeters");
            CheckPositive(issues, source.LocalizationProfile.AcceptanceDistanceMeters, source.ModelId, "LocalizationProfile.AcceptanceDistanceMeters");

            // Le gabarit versionne n'est pas qu'un fini : une demi-largeur ou une longueur nulle ou
            // negative est un profil non renseigne, et la valeur se propagerait telle quelle aux
            // mesures de degagement de la migration (5.27) sans qu'aucun consommateur ne la rejette.
            // La marge laterale, elle, reste authoree sans borne basse : zero y est une decision
            // explicite (« aucune marge exigee »), pas un oubli.
            CheckPositive(issues, source.ValidationProfile.MaxVehicleHalfWidthMeters, source.ModelId, "ValidationProfile.MaxVehicleHalfWidthMeters");
            CheckPositive(issues, source.ValidationProfile.MaxVehicleLengthMeters, source.ModelId, "ValidationProfile.MaxVehicleLengthMeters");

            // AD-48 : « a peu pres parallele ou antiparallele ». Le seuil est une valeur versionnee ;
            // la borne est ouverte a 90 degres, sinon la perpendiculaire passerait.
            float grounding = source.ValidationProfile.GroundingMaxOffAxisDegrees;
            if (IsFinite(grounding) && !(grounding > 0f && grounding < 90f))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.NumericValueOutOfRange,
                    source.ModelId,
                    "ValidationProfile.GroundingMaxOffAxisDegrees hors de ]0, 90[ (" + grounding.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")."));
            }

            // Plafonds approuves du contrat (AD-45) : un profil qui les relache echoue quel que soit
            // le chemin de compilation, meme si le modele est geometriquement coherent.
            CheckApprovedCeiling(issues, source.ValidationProfile.SeamGapToleranceMeters, ApprovedSeamGapCeilingMeters, source.ModelId, "ValidationProfile.SeamGapToleranceMeters");
            CheckApprovedCeiling(issues, source.ValidationProfile.SeamTangentToleranceDegrees, ApprovedSeamTangentCeilingDegrees, source.ModelId, "ValidationProfile.SeamTangentToleranceDegrees");
            CheckApprovedCeiling(issues, source.ValidationProfile.LengthToleranceMeters, ApprovedLengthToleranceCeilingMeters, source.ModelId, "ValidationProfile.LengthToleranceMeters");

            // Contrat AD-45 : le SENS du drapeau `WrongWay` est fixe (erreur de cap absolue au-dela de
            // 90 deg) ; le seuil reste une donnee de profil. Le domaine est donc ]0, 90] : un profil a
            // 179 deg compilait et ne signalait plus un contresens frontal.
            float wrongWay = source.LocalizationProfile.WrongWayHeadingDegrees;
            if (IsFinite(wrongWay) && !(wrongWay > 0f && wrongWay <= 90f))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.NumericValueOutOfRange,
                    source.ModelId,
                    "LocalizationProfile.WrongWayHeadingDegrees hors de ]0, 90] (" + wrongWay.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")."));
            }

            for (int i = 0; i < sections.Length; i++)
            {
                CheckFinite(issues, sections[i].DefaultSpeedLimitMetersPerSecond, sections[i].Id, "RoadSection.DefaultSpeedLimitMetersPerSecond");
            }

            // ------------------------------------------------ corridors
            for (int i = 0; i < corridors.Length; i++)
            {
                var corridor = corridors[i];
                ResolveReference(issues, kindById, corridor.SectionId, RoadRecordKind.Section, corridor.Id, "LaneCorridor.SectionId");
                CheckFinite(issues, corridor.LengthMeters, corridor.Id, "LaneCorridor.LengthMeters");
                if (corridor.HasSpeedLimitOverride)
                {
                    CheckFinite(issues, corridor.SpeedLimitOverrideMetersPerSecond, corridor.Id, "LaneCorridor.SpeedLimitOverrideMetersPerSecond");
                }

                CheckSamples(issues, corridor.Samples, corridor.Id, "LaneCorridor");
            }

            CheckCrossSectionOrder(issues, sections, corridors);

            // ------------------------------------------------ connexions et adjacences
            for (int i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                ResolveReference(issues, kindById, connection.FromCorridorId, RoadRecordKind.Corridor, connection.Id, "LaneConnection.FromCorridorId");
                ResolveReference(issues, kindById, connection.ToCorridorId, RoadRecordKind.Corridor, connection.Id, "LaneConnection.ToCorridorId");
            }

            for (int i = 0; i < adjacencies.Length; i++)
            {
                var adjacency = adjacencies[i];
                ResolveReference(issues, kindById, adjacency.FromCorridorId, RoadRecordKind.Corridor, adjacency.Id, "LaneAdjacency.FromCorridorId");
                ResolveReference(issues, kindById, adjacency.ToCorridorId, RoadRecordKind.Corridor, adjacency.Id, "LaneAdjacency.ToCorridorId");
                CheckFinite(issues, adjacency.FromStartSMeters, adjacency.Id, "LaneAdjacency.FromStartSMeters");
                CheckFinite(issues, adjacency.FromEndSMeters, adjacency.Id, "LaneAdjacency.FromEndSMeters");
                CheckFinite(issues, adjacency.ToStartSMeters, adjacency.Id, "LaneAdjacency.ToStartSMeters");
                CheckFinite(issues, adjacency.ToEndSMeters, adjacency.Id, "LaneAdjacency.ToEndSMeters");
            }

            // ------------------------------------------------ carrefours et mouvements
            for (int i = 0; i < junctions.Length; i++)
            {
                CheckFinite(issues, junctions[i].Boundary.Center, junctions[i].Id, "Junction.Boundary.Center");
                CheckFinite(issues, junctions[i].Boundary.Extents, junctions[i].Id, "Junction.Boundary.Extents");
            }

            var movementJunction = new Dictionary<RoadId, RoadId>();
            for (int i = 0; i < movements.Length; i++)
            {
                var movement = movements[i];

                // Parent unique et persiste : absent, non resolu, ou deja declare avec un autre parent.
                RoadRecordKind parentKind;
                if (movement.JunctionId.IsEmpty
                    || !kindById.TryGetValue(movement.JunctionId, out parentKind)
                    || parentKind != RoadRecordKind.Junction)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.MovementParentInvalid,
                        movement.Id,
                        "JunctionMovement sans carrefour parent resolu (JunctionId=" + movement.JunctionId + ")."));
                }
                else if (!movement.Id.IsEmpty)
                {
                    RoadId knownParent;
                    if (movementJunction.TryGetValue(movement.Id, out knownParent))
                    {
                        if (knownParent != movement.JunctionId)
                        {
                            issues.Add(new RoadModelValidationIssue(
                                RoadModelValidationCode.MovementParentInvalid,
                                movement.Id,
                                "JunctionMovement declare avec plusieurs carrefours parents (" + knownParent + " et " + movement.JunctionId + ")."));
                        }
                    }
                    else
                    {
                        movementJunction.Add(movement.Id, movement.JunctionId);
                    }
                }

                ResolveReference(issues, kindById, movement.FromCorridorId, RoadRecordKind.Corridor, movement.Id, "JunctionMovement.FromCorridorId");
                ResolveReference(issues, kindById, movement.ToCorridorId, RoadRecordKind.Corridor, movement.Id, "JunctionMovement.ToCorridorId");
                CheckFinite(issues, movement.LengthMeters, movement.Id, "JunctionMovement.LengthMeters");
                CheckFinite(issues, movement.RoutePreferenceWeight, movement.Id, "JunctionMovement.RoutePreferenceWeight");
                CheckSamples(issues, movement.Samples, movement.Id, "JunctionMovement");
            }

            // ------------------------------------------------ couverture de controle (AD-46)
            var controlByMovement = new Dictionary<RoadId, RoadId>();
            var doubleCovered = new HashSet<RoadId>();
            for (int i = 0; i < controls.Length; i++)
            {
                var control = controls[i];
                ResolveReference(issues, kindById, control.JunctionId, RoadRecordKind.Junction, control.Id, "JunctionControl.JunctionId");
                if (control.HasStopLine)
                {
                    CheckFinite(issues, control.StopLine.Start, control.Id, "JunctionControl.StopLine.Start");
                    CheckFinite(issues, control.StopLine.End, control.Id, "JunctionControl.StopLine.End");
                }

                var seen = new HashSet<RoadId>();
                var members = Safe(control.ControlledMovementIds);
                for (int m = 0; m < members.Length; m++)
                {
                    RoadId movementId = members[m];
                    if (!ResolveReference(issues, kindById, movementId, RoadRecordKind.Movement, control.Id, "JunctionControl.ControlledMovementIds"))
                    {
                        continue;
                    }

                    if (!seen.Add(movementId))
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.DuplicateId,
                            movementId,
                            "JunctionControl " + control.Id + " liste deux fois le meme mouvement."));
                        continue;
                    }

                    RoadId ownerJunction;
                    if (movementJunction.TryGetValue(movementId, out ownerJunction) && ownerJunction != control.JunctionId)
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.MovementControlCoverageInvalid,
                            movementId,
                            "Mouvement couvert par un JunctionControl d'un autre carrefour (" + control.JunctionId + ")."));
                    }

                    RoadId existingControl;
                    if (controlByMovement.TryGetValue(movementId, out existingControl))
                    {
                        if (doubleCovered.Add(movementId))
                        {
                            issues.Add(new RoadModelValidationIssue(
                                RoadModelValidationCode.MovementControlCoverageInvalid,
                                movementId,
                                "Mouvement couvert par deux JunctionControl (" + existingControl + " et " + control.Id + ") ; la liaison doit etre unique."));
                        }
                    }
                    else
                    {
                        controlByMovement.Add(movementId, control.Id);
                    }
                }
            }

            for (int i = 0; i < movements.Length; i++)
            {
                RoadId movementId = movements[i].Id;
                if (movementId.IsEmpty || controlByMovement.ContainsKey(movementId))
                {
                    continue;
                }

                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.MovementControlCoverageInvalid,
                    movementId,
                    "Mouvement sans JunctionControl ; Uncontrolled est un choix explicite, pas un repli."));
            }

            // ------------------------------------------------ zones de conflit
            for (int i = 0; i < conflictZones.Length; i++)
            {
                var zone = conflictZones[i];
                ResolveReference(issues, kindById, zone.JunctionId, RoadRecordKind.Junction, zone.Id, "ConflictZone.JunctionId");
                CheckFinite(issues, zone.Volume.Center, zone.Id, "ConflictZone.Volume.Center");
                CheckFinite(issues, zone.Volume.Extents, zone.Id, "ConflictZone.Volume.Extents");

                // L'appartenance est la propriete meme du record : une zone possede au moins deux
                // mouvements distincts DU MEME carrefour qu'elle. Sans cette verification, un
                // JunctionId mal saisi ferait silencieusement sauter ValidateConflictingGreens,
                // qui filtre sur zone.JunctionId == plan.JunctionId : la zone serait ignoree et une
                // phase rendant ses deux membres verts compilerait.
                var members = Safe(zone.MemberMovementIds);
                if (members.Length < 2)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.ConflictZoneMembershipInvalid,
                        zone.Id,
                        "ConflictZone declare " + members.Length + " membre(s) ; au moins deux sont exiges."));
                }

                var seenMembers = new HashSet<RoadId>();
                for (int m = 0; m < members.Length; m++)
                {
                    if (!ResolveReference(issues, kindById, members[m], RoadRecordKind.Movement, zone.Id, "ConflictZone.MemberMovementIds"))
                    {
                        continue;
                    }

                    if (!seenMembers.Add(members[m]))
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.DuplicateId,
                            members[m],
                            "ConflictZone " + zone.Id + " liste deux fois le meme mouvement."));
                        continue;
                    }

                    RoadId ownerJunction;
                    if (movementJunction.TryGetValue(members[m], out ownerJunction) && ownerJunction != zone.JunctionId)
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.ConflictZoneMembershipInvalid,
                            members[m],
                            "Membre d'une ConflictZone du carrefour " + zone.JunctionId + " alors qu'il appartient au carrefour " + ownerJunction + "."));
                    }
                }
            }

            // ------------------------------------------------ plans de signal
            var planByJunction = new Dictionary<RoadId, List<SignalPlan>>();
            for (int i = 0; i < signalPlans.Length; i++)
            {
                var plan = signalPlans[i];
                ResolveReference(issues, kindById, plan.JunctionId, RoadRecordKind.Junction, plan.Id, "SignalPlan.JunctionId");
                ValidatePlanStructure(issues, plan, kindById);

                List<SignalPlan> bucket;
                if (!planByJunction.TryGetValue(plan.JunctionId, out bucket))
                {
                    bucket = new List<SignalPlan>();
                    planByJunction.Add(plan.JunctionId, bucket);
                }

                bucket.Add(plan);
            }

            ValidateSignalizedCoverage(issues, controls, planByJunction);
            ValidateConflictingGreens(issues, signalPlans, conflictZones);

            // ------------------------------------------------ portails
            for (int i = 0; i < portals.Length; i++)
            {
                var portal = portals[i];
                ResolveReference(issues, kindById, portal.CorridorId, RoadRecordKind.Corridor, portal.Id, "Portal.CorridorId");
                CheckFinite(issues, portal.SMeters, portal.Id, "Portal.SMeters");
                CheckFinite(issues, portal.EnvelopeLengthMeters, portal.Id, "Portal.EnvelopeLengthMeters");
                CheckFinite(issues, portal.EnvelopeHalfWidthMeters, portal.Id, "Portal.EnvelopeHalfWidthMeters");
            }

            // ------------------------------------------------ lignee fournie avec la source
            ValidateManifest(issues, source, kindById);

            // ------------------------------------------------ geometrie (5.26)
            // Seulement sur une source structurellement propre : la geometrie suppose des references
            // resolues et des echantillons finis a abscisse strictement croissante. Elle fait partie
            // de Validate, donc « vide » signifie toujours « compilable ».
            if (issues.Count == 0)
            {
                RoadGeometryValidator.Validate(source, issues);
            }

            return issues;
        }

        // ------------------------------------------------------------------ helpers

        private static T[] Safe<T>(T[] values)
        {
            return values ?? new T[0];
        }

        private static void RegisterId(
            List<RoadModelValidationIssue> issues,
            Dictionary<RoadId, RoadRecordKind> kindById,
            RoadId id,
            RoadRecordKind kind,
            string typeName)
        {
            if (id.IsEmpty)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, RoadId.None, typeName + " sans identifiant."));
                return;
            }

            if (kindById.ContainsKey(id))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.DuplicateId,
                    id,
                    "Identifiant deja utilise par " + kindById[id] + " ; re-declare par " + typeName + "."));
                return;
            }

            kindById.Add(id, kind);
        }

        private static bool ResolveReference(
            List<RoadModelValidationIssue> issues,
            Dictionary<RoadId, RoadRecordKind> kindById,
            RoadId reference,
            RoadRecordKind expectedKind,
            RoadId owner,
            string fieldName)
        {
            if (reference.IsEmpty)
            {
                issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, owner, fieldName + " est vide."));
                return false;
            }

            RoadRecordKind actualKind;
            if (!kindById.TryGetValue(reference, out actualKind))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.UnresolvedReference,
                    reference,
                    fieldName + " de " + owner + " ne resout aucun enregistrement."));
                return false;
            }

            if (actualKind != expectedKind)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.UnresolvedReference,
                    reference,
                    fieldName + " de " + owner + " attend " + expectedKind + " et resout " + actualKind + "."));
                return false;
            }

            return true;
        }

        private static void CheckFinite(List<RoadModelValidationIssue> issues, float value, RoadId subject, string fieldName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.NonFiniteNumericValue,
                    subject,
                    fieldName + " n'est pas fini (" + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")."));
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        /// <summary>Strictement positif ; une valeur non finie est deja rapportee par CheckFinite.</summary>
        private static void CheckPositive(List<RoadModelValidationIssue> issues, float value, RoadId subject, string fieldName)
        {
            if (IsFinite(value) && !(value > 0f))
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.NumericValueOutOfRange,
                    subject,
                    fieldName + " doit etre strictement positif (" + value.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")."));
            }
        }

        // ------------------------------------------------------------------ plafonds approuves

        /// <summary>Plafond approuve de l'ecart de position ou de largeur a une couture, en metres.</summary>
        private const float ApprovedSeamGapCeilingMeters = 0.05f;

        /// <summary>Plafond approuve de l'ecart de tangente a une couture, en degres (AD-45).</summary>
        private const float ApprovedSeamTangentCeilingDegrees = 5f;

        /// <summary>
        /// Plafond approuve de la tolerance de longueur, en metres : elle borne aussi l'ecart
        /// abscisse/corde et le domaine des portails, tous deux plafonnes a 0,05 m par AD-45.
        /// </summary>
        private const float ApprovedLengthToleranceCeilingMeters = 0.05f;

        /// <summary>
        /// Un profil plus strict que le contrat passe ; le relacher est un echec dur a code stable,
        /// qui nomme le champ fautif. La revision d'un plafond est une decision d'architecture.
        /// </summary>
        private static void CheckApprovedCeiling(
            List<RoadModelValidationIssue> issues,
            float value,
            float ceiling,
            RoadId subject,
            string fieldName)
        {
            if (IsFinite(value) && value > ceiling)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.ProfileToleranceAboveApprovedCeiling,
                    subject,
                    fieldName + " = " + value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                        + " relache le plafond approuve de " + ceiling.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                        + " ; une revision du contrat passe par l'architecture, jamais par un profil."));
            }
        }

        private static void CheckFinite(List<RoadModelValidationIssue> issues, Vector3 value, RoadId subject, string fieldName)
        {
            CheckFinite(issues, value.x, subject, fieldName + ".x");
            CheckFinite(issues, value.y, subject, fieldName + ".y");
            CheckFinite(issues, value.z, subject, fieldName + ".z");
        }

        /// <summary>
        /// Ordre transversal authore (AD-48) : par section, exactement un corridor datum, des
        /// <c>LateralOrder</c> uniques et contigus depuis 0.
        ///
        /// <b>Purement structurel.</b> Aucune mathematique de courbe, aucun road-right, aucune
        /// projection : on ne verifie ici que la forme de l'ensemble. La coherence entre cet ordre
        /// et la geometrie reelle -- monotonie le long du datum, recouvrement d'enveloppes, accord
        /// avec <see cref="LaneAdjacency"/> -- appartient a la Story 5.26.
        ///
        /// Seuls les corridors dont la section resout sont groupes : un <c>SectionId</c> non resolu
        /// est deja un echec dur (<see cref="RoadModelValidationCode.UnresolvedReference"/>) et le
        /// compter ici ne produirait que du bruit. Une section sans aucun corridor n'a pas de coupe
        /// transversale et n'exige donc pas de datum.
        /// </summary>
        private static void CheckCrossSectionOrder(
            List<RoadModelValidationIssue> issues,
            RoadSection[] sections,
            LaneCorridor[] corridors)
        {
            var known = new HashSet<RoadId>();
            for (int i = 0; i < sections.Length; i++)
            {
                if (!sections[i].Id.IsEmpty)
                {
                    known.Add(sections[i].Id);
                }
            }

            var bySection = new Dictionary<RoadId, List<LaneCorridor>>();
            for (int i = 0; i < corridors.Length; i++)
            {
                if (!known.Contains(corridors[i].SectionId))
                {
                    continue;
                }

                List<LaneCorridor> group;
                if (!bySection.TryGetValue(corridors[i].SectionId, out group))
                {
                    group = new List<LaneCorridor>();
                    bySection.Add(corridors[i].SectionId, group);
                }

                group.Add(corridors[i]);
            }

            // Ordre de parcours deterministe : la liste d'echecs ne doit pas dependre du hachage.
            for (int i = 0; i < sections.Length; i++)
            {
                List<LaneCorridor> group;
                if (sections[i].Id.IsEmpty || !bySection.TryGetValue(sections[i].Id, out group))
                {
                    continue;
                }

                CheckOneCrossSection(issues, sections[i].Id, group);
            }
        }

        private static void CheckOneCrossSection(
            List<RoadModelValidationIssue> issues,
            RoadId sectionId,
            List<LaneCorridor> group)
        {
            // ---------------------------------------------------------- datum unique
            int datumCount = 0;
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].IsCrossSectionDatum)
                {
                    datumCount++;
                }
            }

            if (datumCount == 0)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.MissingCrossSectionDatum,
                    sectionId,
                    "RoadSection sans corridor datum : " + group.Count + " corridor(s), aucun IsCrossSectionDatum."));
            }
            else if (datumCount > 1)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.MultipleCrossSectionData,
                    sectionId,
                    "RoadSection avec " + datumCount + " corridors datum : le repere transversal doit en designer exactement un."));
            }

            // ---------------------------------------------------------- ordres uniques
            bool duplicated = false;
            var seen = new Dictionary<int, RoadId>(group.Count);
            for (int i = 0; i < group.Count; i++)
            {
                RoadId first;
                if (seen.TryGetValue(group[i].LateralOrder, out first))
                {
                    duplicated = true;
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.DuplicateLateralOrder,
                        group[i].Id,
                        "LateralOrder " + group[i].LateralOrder + " deja porte par " + first + " dans la section " + sectionId + "."));
                }
                else
                {
                    seen.Add(group[i].LateralOrder, group[i].Id);
                }
            }

            // Contiguite indefinie tant qu'un ordre est duplique : ne pas empiler un second motif
            // sur le meme defaut.
            if (duplicated)
            {
                return;
            }

            // ---------------------------------------------------------- contigus depuis 0
            for (int order = 0; order < group.Count; order++)
            {
                if (!seen.ContainsKey(order))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.NonContiguousLateralOrder,
                        sectionId,
                        "RoadSection de " + group.Count + " corridor(s) sans LateralOrder " + order
                            + " : la coupe transversale doit etre contigue depuis 0."));
                }
            }
        }

        private static void CheckSamples(List<RoadModelValidationIssue> issues, RoadCurveSample[] samples, RoadId subject, string typeName)
        {
            if (samples == null || samples.Length < 2)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.InvalidGeometryPayload,
                    subject,
                    typeName + " exige au moins deux echantillons de courbe."));
                return;
            }

            for (int i = 0; i < samples.Length; i++)
            {
                var sample = samples[i];
                CheckFinite(issues, sample.SMeters, subject, typeName + ".Samples[" + i + "].SMeters");
                CheckFinite(issues, sample.Position, subject, typeName + ".Samples[" + i + "].Position");
                CheckFinite(issues, sample.Tangent, subject, typeName + ".Samples[" + i + "].Tangent");
                CheckFinite(issues, sample.Up, subject, typeName + ".Samples[" + i + "].Up");
                CheckFinite(issues, sample.CurvaturePerMeter, subject, typeName + ".Samples[" + i + "].CurvaturePerMeter");
                CheckFinite(issues, sample.HalfWidthLeftMeters, subject, typeName + ".Samples[" + i + "].HalfWidthLeftMeters");
                CheckFinite(issues, sample.HalfWidthRightMeters, subject, typeName + ".Samples[" + i + "].HalfWidthRightMeters");

                // L'ordre des echantillons est semantique (abscisse croissante), pas un ordre de
                // collection : c'est ce qui autorise le serialiseur canonique a le conserver.
                if (i > 0 && !(sample.SMeters > samples[i - 1].SMeters))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.InvalidGeometryPayload,
                        subject,
                        typeName + ".Samples n'est pas strictement croissant en s a l'index " + i + "."));
                }
            }
        }

        private static void ValidatePlanStructure(
            List<RoadModelValidationIssue> issues,
            SignalPlan plan,
            Dictionary<RoadId, RoadRecordKind> kindById)
        {
            var groups = Safe(plan.Groups);
            var groupIds = new HashSet<RoadId>();
            for (int g = 0; g < groups.Length; g++)
            {
                var group = groups[g];
                if (group.GroupId.IsEmpty)
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, plan.Id, "SignalPlan contient un groupe sans identifiant."));
                    continue;
                }

                if (!groupIds.Add(group.GroupId))
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.DuplicateId, group.GroupId, "SignalPlan " + plan.Id + " declare deux fois ce groupe."));
                }

                var members = Safe(group.MemberMovementIds);
                for (int m = 0; m < members.Length; m++)
                {
                    ResolveReference(issues, kindById, members[m], RoadRecordKind.Movement, plan.Id, "SignalGroup.MemberMovementIds");
                }
            }

            var phases = Safe(plan.Phases);
            if (phases.Length == 0)
            {
                issues.Add(new RoadModelValidationIssue(
                    RoadModelValidationCode.SignalizedControlWithoutPlan,
                    plan.Id,
                    "SignalPlan sans phase."));
                return;
            }

            var phaseIds = new HashSet<RoadId>();
            for (int p = 0; p < phases.Length; p++)
            {
                var phase = phases[p];
                if (phase.PhaseId.IsEmpty)
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, plan.Id, "SignalPlan contient une phase sans identifiant."));
                }
                else if (!phaseIds.Add(phase.PhaseId))
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.DuplicateId, phase.PhaseId, "SignalPlan " + plan.Id + " declare deux fois cette phase."));
                }

                CheckFinite(issues, phase.DurationSeconds, plan.Id, "SignalPhase.DurationSeconds");
                if (!float.IsNaN(phase.DurationSeconds) && !float.IsInfinity(phase.DurationSeconds) && phase.DurationSeconds <= 0f)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.SignalizedControlWithoutPlan,
                        plan.Id,
                        "SignalPhase " + phase.PhaseId + " a une duree non positive."));
                }

                var states = Safe(phase.GroupStates);
                if (states.Length == 0)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.SignalizedControlWithoutPlan,
                        plan.Id,
                        "SignalPhase " + phase.PhaseId + " est vide."));
                }

                for (int s = 0; s < states.Length; s++)
                {
                    if (!groupIds.Contains(states[s].GroupId))
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.UnresolvedReference,
                            states[s].GroupId,
                            "SignalPhase " + phase.PhaseId + " reference un groupe absent du plan " + plan.Id + "."));
                    }
                }
            }
        }

        private static void ValidateSignalizedCoverage(
            List<RoadModelValidationIssue> issues,
            JunctionControl[] controls,
            Dictionary<RoadId, List<SignalPlan>> planByJunction)
        {
            for (int i = 0; i < controls.Length; i++)
            {
                var control = controls[i];
                if (control.Kind != JunctionControlKind.Signalized)
                {
                    continue;
                }

                List<SignalPlan> plans;
                if (!planByJunction.TryGetValue(control.JunctionId, out plans) || plans.Count != 1)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.SignalizedControlWithoutPlan,
                        control.Id,
                        "Controle Signalized sans SignalPlan unique sur le carrefour " + control.JunctionId + "."));
                    continue;
                }

                var plan = plans[0];
                var groups = Safe(plan.Groups);
                var members = Safe(control.ControlledMovementIds);
                for (int m = 0; m < members.Length; m++)
                {
                    int cover = 0;
                    for (int g = 0; g < groups.Length; g++)
                    {
                        var groupMembers = Safe(groups[g].MemberMovementIds);
                        for (int k = 0; k < groupMembers.Length; k++)
                        {
                            if (groupMembers[k] == members[m])
                            {
                                cover++;
                                break;
                            }
                        }
                    }

                    if (cover != 1)
                    {
                        issues.Add(new RoadModelValidationIssue(
                            RoadModelValidationCode.SignalizedControlWithoutPlan,
                            members[m],
                            "Mouvement signalise couvert par " + cover + " groupe(s) du plan " + plan.Id + " ; exactement un est exige."));
                    }
                }
            }
        }

        private static void ValidateConflictingGreens(
            List<RoadModelValidationIssue> issues,
            SignalPlan[] signalPlans,
            ConflictZone[] conflictZones)
        {
            for (int i = 0; i < signalPlans.Length; i++)
            {
                var plan = signalPlans[i];
                var groups = Safe(plan.Groups);
                var phases = Safe(plan.Phases);

                for (int p = 0; p < phases.Length; p++)
                {
                    var green = new HashSet<RoadId>();
                    var states = Safe(phases[p].GroupStates);
                    for (int s = 0; s < states.Length; s++)
                    {
                        if (states[s].State != SignalState.Green)
                        {
                            continue;
                        }

                        for (int g = 0; g < groups.Length; g++)
                        {
                            if (groups[g].GroupId != states[s].GroupId)
                            {
                                continue;
                            }

                            var members = Safe(groups[g].MemberMovementIds);
                            for (int m = 0; m < members.Length; m++)
                            {
                                green.Add(members[m]);
                            }
                        }
                    }

                    for (int z = 0; z < conflictZones.Length; z++)
                    {
                        var zone = conflictZones[z];
                        if (zone.JunctionId != plan.JunctionId)
                        {
                            continue;
                        }

                        var zoneMembers = Safe(zone.MemberMovementIds);
                        int greenCount = 0;
                        for (int m = 0; m < zoneMembers.Length; m++)
                        {
                            if (green.Contains(zoneMembers[m]))
                            {
                                greenCount++;
                            }
                        }

                        if (greenCount >= 2)
                        {
                            issues.Add(new RoadModelValidationIssue(
                                RoadModelValidationCode.ConflictingMovementsGreenTogether,
                                zone.Id,
                                "La phase " + phases[p].PhaseId + " du plan " + plan.Id + " rend verts " + greenCount + " membres de cette zone de conflit."));
                        }
                    }
                }
            }
        }

        private static void ValidateManifest(
            List<RoadModelValidationIssue> issues,
            RoadModelSource source,
            Dictionary<RoadId, RoadRecordKind> kindById)
        {
            var entries = Safe(source.Manifest.Entries);
            var slots = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                if (entry.RecordId.IsEmpty)
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, RoadId.None, "ImportManifestEntry sans identifiant d'enregistrement."));
                }
                else if (!kindById.ContainsKey(entry.RecordId))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.UnresolvedReference,
                        entry.RecordId,
                        "ImportManifestEntry trace un enregistrement absent du modele."));
                }
                else if (kindById[entry.RecordId] != entry.Kind)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.UnresolvedReference,
                        entry.RecordId,
                        "ImportManifestEntry annonce " + entry.Kind + " pour un enregistrement " + kindById[entry.RecordId] + "."));
                }

                if (entry.ModelId != source.ModelId)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.CrossVersionReference,
                        entry.RecordId,
                        "ImportManifestEntry appartient au modele " + entry.ModelId + ", pas a " + source.ModelId + "."));
                }

                if (string.IsNullOrEmpty(entry.ImporterSlot))
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, entry.RecordId, "ImportManifestEntry sans slot d'importeur."));
                }
                else if (!slots.Add(entry.ImporterSlot))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.DuplicateId,
                        entry.RecordId,
                        "Slot d'importeur deja utilise : " + entry.ImporterSlot + "."));
                }
            }

            var tombstones = Safe(source.Manifest.TombstonedIds);
            var tombstoneSet = new HashSet<RoadId>();
            for (int i = 0; i < tombstones.Length; i++)
            {
                if (tombstones[i].IsEmpty)
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, RoadId.None, "Tombstone sans identifiant."));
                    continue;
                }

                tombstoneSet.Add(tombstones[i]);
                if (kindById.ContainsKey(tombstones[i]))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.TombstonedIdReused,
                        tombstones[i],
                        "Un enregistrement vivant reutilise un identifiant supprime."));
                }
            }

            var remaps = Safe(source.Manifest.Remaps);
            var remapSources = new HashSet<RoadId>();
            for (int i = 0; i < remaps.Length; i++)
            {
                var remap = remaps[i];
                if (remap.FromId.IsEmpty || remap.ToId.IsEmpty)
                {
                    issues.Add(new RoadModelValidationIssue(RoadModelValidationCode.EmptyId, remap.ToId, "Remap de manifeste avec un identifiant vide."));
                    continue;
                }

                if (remap.FromId == remap.ToId)
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.IncompatibleManifestRemap,
                        remap.FromId,
                        "Remap de manifeste vers lui-meme."));
                    continue;
                }

                if (!remapSources.Add(remap.FromId))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.IncompatibleManifestRemap,
                        remap.FromId,
                        "Deux remaps de manifeste partent du meme identifiant."));
                }

                if (kindById.ContainsKey(remap.FromId))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.IncompatibleManifestRemap,
                        remap.FromId,
                        "Remap de manifeste depuis un identifiant encore vivant."));
                }

                if (!kindById.ContainsKey(remap.ToId))
                {
                    issues.Add(new RoadModelValidationIssue(
                        RoadModelValidationCode.IncompatibleManifestRemap,
                        remap.ToId,
                        "Remap de manifeste vers un identifiant absent du modele."));
                }
            }
        }
    }
}
