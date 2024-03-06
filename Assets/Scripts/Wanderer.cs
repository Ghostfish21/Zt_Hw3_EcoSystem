using System;
using UnityEngine;

namespace DefaultNamespace {
    public class Wanderer {
        public readonly Action changePositionAction;
        public readonly Func<(bool, bool)> lookingForAction;
        public readonly Action resetWanderingAction;
        
        public Wanderer(Action changePositionAction, Func<(bool, bool)> lookingForAction, Action resetWanderingAction) {
            this.changePositionAction = changePositionAction;
            this.lookingForAction = lookingForAction;
            this.resetWanderingAction = resetWanderingAction;
        }

        public bool setup() {
            (bool stopWander, bool resetWander) result = lookingForAction();
            if (!result.stopWander) resetWanderingAction();
            return result.stopWander;
        }

        public bool step() {
            changePositionAction();
            (bool stopWander, bool resetWander) result = lookingForAction();
            if (result.resetWander) resetWanderingAction();
            return result.stopWander;
        }
        
        
    }
}