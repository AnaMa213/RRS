using UnityEngine;

namespace RoadRage.Features.Vehicles
{
    /// <summary>
    /// Story 5.18 : PRESENTATION seule d'une tete de feu. Le composant teinte ses lampes selon la phase
    /// resolue du plan de feux de la jonction, et rien d'autre : il ne lit ni n'ecrit aucun etat de
    /// gameplay, ne touche a aucune <c>NetworkVariable</c>, n'emet aucune RPC et ne porte aucun collider.
    ///
    /// Il n'y a donc rien a repliquer : la phase est une fonction du TEMPS sur une donnee partagee
    /// (<see cref="TrafficSettingsDef.SignalPlans"/>), donc chaque pair en tire la meme conclusion sans
    /// se parler. C'est ce qui evite le second objet d'etat reseau que la story s'interdit.
    ///
    /// Les lampes sont des <see cref="Renderer"/> EXPLICITES, jamais les sous-materiaux d'un mesh tiers :
    /// mesure du 2026-09-20, le prop Synty de feu de circulation porte UN SEUL materiau, donc ses lampes
    /// ne sont pas separables sans chirurgie de mesh. La silhouette Synty reste du decor, la phase se lit
    /// sur les lampes authorées posees a cote.
    ///
    /// Limite connue et assumee : l'horloge est locale (<c>Time.time</c>), donc un client dont l'origine
    /// de temps differe de celle de l'hote peut voir un decalage de phase. C'est de la presentation : le
    /// seul consommateur autoritaire est le conducteur hote, qui evalue la phase sur une horloge unique.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrafficSignalLampView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField]
        [Tooltip("Id de jonction du plan de feux a lire. Vide, ou inconnu du Def de trafic : les lampes gardent leur aspect authore.")]
        private string junctionId = string.Empty;

        [SerializeField]
        [Tooltip("Groupe d'approche que cette tete de feu autorise. C'est l'approche qui lit SA tete, jamais celle d'une autre.")]
        private int signalGroup;

        [SerializeField]
        [Tooltip("Lampe qui s'allume quand la phase courante autorise ce groupe.")]
        private Renderer greenLamp;

        [SerializeField]
        [Tooltip("Lampe qui s'allume quand la phase courante ne l'autorise pas.")]
        private Renderer redLamp;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Cadence de rafraichissement (s). Un feu change au rythme des phases : rien ne justifie une ecriture par frame.")]
        private float refreshInterval = 0.1f;

        [SerializeField]
        private Color greenTint = new Color(0.16f, 0.9f, 0.28f, 1f);

        [SerializeField]
        private Color redTint = new Color(0.9f, 0.16f, 0.16f, 1f);

        [SerializeField]
        [Tooltip("Teinte d'une lampe eteinte. Elle reste visible -- une lampe noire disparaitrait dans le decor.")]
        private Color offTint = new Color(0.1f, 0.1f, 0.1f, 1f);

        private LaneGraph laneGraph;
        private MaterialPropertyBlock propertyBlock;
        private float elapsedSeconds;
        private bool hasAppliedState;
        private bool litGreen;

        private void Awake()
        {
            // Un module est pose sous la racine du graphe : la reference se resout par la hierarchie, donc
            // aucune reference de scene a cabler dans un prefab (qui ne peut pas en porter).
            laneGraph = GetComponentInParent<LaneGraph>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            elapsedSeconds = 0f;
            hasAppliedState = false;
            Apply();
        }

        private void Update()
        {
            elapsedSeconds += Time.deltaTime;
            var interval = Mathf.Max(0f, refreshInterval);
            if (elapsedSeconds < interval) return;
            elapsedSeconds = 0f;
            Apply();
        }

        /// <summary>
        /// Lit la phase courante et teinte les deux lampes. Un plan absent, un id inconnu ou un plan
        /// inexploitable laissent l'aspect authore intact : le feu ne signifie rien, il ne ment pas.
        /// </summary>
        private void Apply()
        {
            if (laneGraph == null || greenLamp == null || redLamp == null) return;

            var settings = laneGraph.TrafficSettings;
            if (settings == null || !settings.TryGetSignalPlan(junctionId, out var plan)) return;
            var phase = JunctionRules.ResolvePhaseIndex(plan, Time.time);
            if (phase < 0) return;

            var green = plan.Phases[phase].AllowsGroup(signalGroup);
            if (hasAppliedState && litGreen == green) return;

            hasAppliedState = true;
            litGreen = green;
            Tint(greenLamp, green ? greenTint : offTint);
            Tint(redLamp, green ? offTint : redTint);
        }

        private void Tint(Renderer lamp, Color color)
        {
            if (lamp == null) return;
            lamp.GetPropertyBlock(propertyBlock);
            // Les trois noms couvrent URP/Lit et le shader integre : ecrire une propriete absente d'un
            // bloc de proprietes est sans effet, ce n'est pas une erreur.
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            propertyBlock.SetColor(EmissionColorId, color);
            lamp.SetPropertyBlock(propertyBlock);
        }
    }
}
