using System;
using System.Collections.Generic;
using RoadRage.Features.Vehicles.Traffic.Frame;

namespace RoadRage.Features.Vehicles.Traffic.Signals
{
    /// <summary>
    /// Horloge de phase des feux, etat hote (Story 5.36). Le modele ne possede que le plan ; la phase courante et le temps ecoule
    /// vivent ici et entrent dans la decision par <see cref="TrafficFrame"/>. Deterministe : chaque plan demarre a sa phase 0,
    /// temps cumule en double, phases enchainees dans l'ordre du plan puis cycliquement. Aucun plan : liste vide, aucun travail.
    /// </summary>
    public sealed class SignalPhaseController
    {
        private readonly CompiledSignalPlan[] plans;
        private readonly int[] phaseIndex;
        private readonly double[] elapsed;
        private readonly double[] cycle;
        private readonly SignalPhaseInput[] current;

        public SignalPhaseController(CompiledRoadModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            Model = model;
            plans = new CompiledSignalPlan[model.SignalPlans.Count];
            for (int i = 0; i < plans.Length; i++) plans[i] = model.SignalPlans[i];
            Array.Sort(plans, (a, b) => a.Id.CompareTo(b.Id));
            phaseIndex = new int[plans.Length];
            elapsed = new double[plans.Length];
            cycle = new double[plans.Length];
            for (int i = 0; i < plans.Length; i++)
                for (int p = 0; p < plans[i].Phases.Count; p++) cycle[i] += plans[i].Phases[p].DurationSeconds;
            current = new SignalPhaseInput[plans.Length];
            for (int i = 0; i < plans.Length; i++) Publish(i);
            Current = Array.AsReadOnly(current);
        }

        public CompiledRoadModel Model { get; }

        /// <summary>Phase courante de chaque plan, triee par PlanId ; vue vivante, copiee par chaque TrafficFrame.</summary>
        public IReadOnlyList<SignalPhaseInput> Current { get; }

        /// <summary>Avance chaque plan de <paramref name="seconds"/> (pas hote) ; un pas non fini ou &lt;= 0 est refuse.</summary>
        public void Advance(float seconds)
        {
            if (!(seconds > 0f) || float.IsInfinity(seconds)) throw new ArgumentException("InvalidSignalStep", "seconds");
            for (int i = 0; i < plans.Length; i++)
            {
                var phases = plans[i].Phases;
                if (phases.Count == 0) continue;
                elapsed[i] += seconds;
                // Durees strictement positives (validateur) ; le reste du cycle borne la boucle a un tour, meme pour des durees
                // si petites qu'une soustraction serait absorbee par l'arrondi.
                if (elapsed[i] >= cycle[i]) elapsed[i] %= cycle[i];
                while (elapsed[i] >= phases[phaseIndex[i]].DurationSeconds)
                {
                    elapsed[i] -= phases[phaseIndex[i]].DurationSeconds;
                    phaseIndex[i] = (phaseIndex[i] + 1) % phases.Count;
                }
                Publish(i);
            }
        }

        private void Publish(int plan)
        {
            var phases = plans[plan].Phases;
            current[plan] = new SignalPhaseInput(plans[plan].Id, phases.Count == 0 ? RoadId.None : phases[phaseIndex[plan]].PhaseId);
        }
    }
}
