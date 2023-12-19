using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace Vampirism
{
    public enum HandType
    {
        Left,
        Right,
        Both
    }

    public enum HandDirection
    {
        Palm,
        Back,
        Fingers,
        Wrist,
        Thumb,
        LittleFinger
    }

    public enum GestureDirection
    {
        Any,
        Forward,
        Backward,
        Up,
        Down,
        Left,
        Right,
        Inside,
        Outside,
        Together,
        Apart
    }

    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise,
        Either
    }

    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    public enum CreatureType
    {
        Player,
        Thrall,
        Other
    }

    public class ControlGesture
    {
        // VARIABLES
        /// <summary>Key for log checks</summary>
        private string debugKey { get => typeof(ControlGesture).Name + ": "; }
        private float directionAngleTolerance = 40.0f;

        private Dictionary<VampireAbilityEnum, int> abilityLevelsRequired;

        private RagdollHand[] hands; // hands to check in the gesture
        private List<(HandDirection handDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> facingDirection)> originDirection)> partFacing; // Check if part of hand is facing given direction based on origin transform's local space
        private List<(Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)> originDirection, float magnitude)> moving; // Check if hand is moving in given direction at or above certain magnitude based on origin transform's local space
        private List<(Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)> originDirection, float distance)> offsets; // Check if hand is offset from origin transform in given direction at given distance based on origin transform's local space
        private (HandDirection rotationUpAxis, RotationDirection rotationDirection, float angle)? twist; // Check if hand has been rotated a certain angle in the given direction based on a given up axis (from hand)
        private bool still = false; // Check if hand needs to be still (false = does not need to be still, true = needs to be still)
        private bool? gripping = new bool?(); // Check if hand needs to be gripping (null = no check, false = not gripping, true = gripping)
        private bool? triggering = new bool?(); // Check if hand needs to be pressing trigger (null = no check, false = not pressing trigger, true = pressing trigger)
        private bool? altPressing = new bool?(); // Check if hand needs to be pressing alt use (null = no check, false = not pressing alt use, true = pressing alt use)
        private bool? casting = new bool?(); // Check if hand is casting any spell (null = no check, false = not casting, true = casting)
        private (string spellID, bool isActive)? spellActive; // Check if spell is loaded to checked hand (string = spell id, bool = is active)
        private (string spellID, bool isCasting)? spellCasting; // Check if spell is being cast on checked hand (string = spell id, bool = is casting)
        private bool? holding = new bool?(); // Check if hand is holding anything (null = no check, false = not holding, true = holding)
        private bool? holdingItem = new bool?(); // Check if hand is holding an item (null = no check, false = not holding item, true = holding item)
        private ItemData.Type? holdingItemType; // Check if hand is holding item of given type (null = no check)
        private string holdingItemSpecific_ID; // Check if hand is holding item with given id
        private bool? holdingCreature = new bool?(); // Check if hand is holding a creature (null = no check, false = not holding creature, true = holding creature)
        private CreatureType? holdingCreatureType; // Check if hand is holding creature of given type (null = no check)
        
        // FUNCTIONS/PROPERTIES
        protected ControlGesture(HandType handType)
        {
            switch (handType)
            {
                case HandType.Left:
                    hands = new RagdollHand[] { VampireMaster.local.PlayerHand(Side.Left) };
                    break;
                case HandType.Right:
                    hands = new RagdollHand[] { VampireMaster.local.PlayerHand(Side.Right) };
                    break;
                case HandType.Both:
                    hands = new RagdollHand[] { VampireMaster.local.PlayerHand(Side.Left), VampireMaster.local.PlayerHand(Side.Right) };
                    break;
                default:
                    hands = new RagdollHand[0];
                    Debug.LogError(debugKey + "Constructor handType did not match available values");
                    break;
            }

            abilityLevelsRequired = new Dictionary<VampireAbilityEnum, int>();
        }

        public static ControlGesture Left { get => new ControlGesture(HandType.Left); }

        public static ControlGesture Right { get => new ControlGesture(HandType.Right); }

        public static ControlGesture Both { get => new ControlGesture(HandType.Both); }

        //============
        // CONDITIONS
        //============
        public ControlGesture AbilityUnlocked(VampireAbilityEnum abilityEnum, int level)
        {
            if (abilityLevelsRequired.ContainsKey(abilityEnum))
            {
                abilityLevelsRequired[abilityEnum] = level;
            }
            else
            {
                abilityLevelsRequired.Add(abilityEnum, level);
            }

            return this;
        }

        public ControlGesture AbilityUnlocked(params Tuple<VampireAbilityEnum, int>[] abilityLevels)
        {
            if (abilityLevels == null || abilityLevels.Length <= 0) return this;
            
            for (int i = 0; i < abilityLevels.Length; ++i)
            {
                Tuple<VampireAbilityEnum, int> level = abilityLevels[i];
                AbilityUnlocked(level.Item1, level.Item2);
            }

            return this;
        }

        public ControlGesture Facing(HandDirection handDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> direction)> originDirection)
        {
            if (partFacing == null) partFacing = new List<(HandDirection handDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> direction)> originTransform)>();

            partFacing.Add((handDirection, originDirection));

            return this;
        }

        public ControlGesture Facing(params Tuple<HandDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> direction)>>[] facingDirections)
        {
            if (facingDirections == null || facingDirections.Length <= 0) return this;

            for (int i = 0; i < facingDirections.Length; ++i)
            {
                Tuple<HandDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> direction)>> facing = facingDirections[i];
                Facing(facing.Item1, facing.Item2);
            }

            return this;
        }

        public ControlGesture Moving(Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)> originDirection, float moveSpeed = 0.02f)
        {
            if (moving == null) moving = new List<(Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)> originDirection, float magnitude)>();

            moving.Add((originDirection, moveSpeed));

            return this;
        }

        public ControlGesture Moving(params Tuple<Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)>, float>[] moves)
        {
            if (moves == null || moves.Length <= 0) return this;

            for (int i = 0; i < moves.Length; ++i)
            {
                Tuple<Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)>, float> move = moves[i];
                Moving(move.Item1, move.Item2);
            }

            return this;
        }

        public ControlGesture Offset(Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)> originDirection, float amount)
        {
            if (offsets == null) offsets = new List<(Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)> originDirection, float distance)>();

            offsets.Add((originDirection, amount));

            return this;
        }

        public ControlGesture Offset(params Tuple<Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)>, float>[] offset)
        {
            if (offset == null || offset.Length <= 0) return this;

            for (int i = 0; i < offset.Length; ++i)
            {
                Tuple<Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)>, float> newOffset = offset[i];
                Offset(newOffset.Item1, newOffset.Item2);
            }

            return this;
        }
        
        public ControlGesture Twist(HandDirection rotationAxisUp, RotationDirection rotationDirection, float angle)
        {
            twist = (rotationAxisUp, rotationDirection, angle);
            return this;
        }

        public ControlGesture Still(bool isTrue)
        {
            still = isTrue;
            return this;
        }

        public ControlGesture Gripping(bool? isTrue)
        {
            gripping = isTrue;
            return this;
        }

        public ControlGesture Triggering(bool? isTrue)
        {
            triggering = isTrue;
            return this;
        }

        public ControlGesture Open
        {
            get
            {
                return Gripping(false).Triggering(false);
            }
        }

        public ControlGesture Fist
        {
            get
            {
                return Gripping(true).Triggering(true);
            }
        }

        public ControlGesture AltPressing(bool? isTrue)
        {
            altPressing = isTrue;
            return this;
        }

        public ControlGesture Casting(bool? isTrue)
        {
            casting = isTrue;
            return this;
        }

        public ControlGesture SpellActive(string spell, bool active)
        {
            spellActive = (spell, active);
            return this;
        }

        public ControlGesture SpellCasting(string spell, bool casting)
        {
            spellCasting = (spell, casting);
            return this;
        }

        public ControlGesture Holding(bool? isTrue)
        {
            holding = isTrue;
            return this;
        }

        public ControlGesture HoldingItem(bool? isTrue)
        {
            holdingItem = isTrue;
            return this;
        }

        public ControlGesture HoldingItemType(ItemData.Type? itemType)
        {
            holdingItemType = itemType;
            return this;
        }

        /// <summary>
        /// Check if the hand(s) are holding an item with its id matching the provided string
        /// </summary>
        /// <param name="id">The id of the item that the hand(s) should be holding (null/empty/etc = no check)</param>
        /// <returns>Base ControlGesture with Holding Item Specific check added/changed</returns>
        public ControlGesture HoldingItemSpecific(string id)
        {
            holdingItemSpecific_ID = id;
            return this;
        }

        /// <summary>
        /// Check if the hand(s) are holding a creature or not
        /// </summary>
        /// <param name="isTrue">Should the hand(s) be holding a creature (null = no check)</param>
        /// <returns>Base ControlGesture with Holding Creature check added/changed</returns>
        public ControlGesture HoldingCreature(bool? isTrue)
        {
            holdingCreature = isTrue;
            return this;
        }

        /// <summary>
        /// Check if the hand(s) are holding a creature of the specified type
        /// </summary>
        /// <param name="creatureType">The type of creature the hand(s) should be holding (null = no check)</param>
        /// <returns>Base ControlGesture with Holding Creature Type check added/changed</returns>
        public ControlGesture HoldingCreatureType(CreatureType? creatureType)
        {
            holdingCreatureType = creatureType;
            return this;
        }

        //========
        // OUTPUT
        //========

        /// <summary>
        /// Returns the string description for the gesture being checked for
        /// </summary>
        public string Description
        {
            get
            {
                string description;

                // HANDS
                if (hands.Length > 1) description = "Both hands ";
                else description = hands[0].side.ToString() + " hand ";

                // ABILITY LEVEL
                foreach (KeyValuePair<VampireAbilityEnum, int> kvp in abilityLevelsRequired)
                {
                    description += "\n+ " + kvp.Key.ToString() + " at or above level " + kvp.Value.ToString() + " ";
                }


                for (int i = 0; i < hands.Length; ++i)
                {
                    RagdollHand hand = hands[i];
                    
                    // PART FACING
                    if (partFacing != null)
                    {
                        for (int j = 0; j < partFacing.Count; ++j)
                        {
                            description += "\n+ " + hand.side.ToString() + " " + partFacing[j].handDirection.ToString() + " facing " + partFacing[j].originDirection(hand).facingDirection().ToString() + " based on origin transform: " + (partFacing[j].originDirection(hand).localOrigin == null ? "INVALID" : partFacing[j].originDirection(hand).localOrigin.gameObject.name); 
                        }
                    }

                    // MOVING
                    if (moving != null)
                    {
                        for (int j = 0; j < moving.Count; ++j)
                        {
                            description += "\n+ " + hand.side.ToString() + " hand moving " + moving[j].originDirection(hand).movingDirection().ToString() + " at or above magnitude " + moving[j].magnitude.ToString() + " based on origin transform: " + (moving[j].originDirection(hand).localOrigin == null ? "INVALID" : moving[j].originDirection(hand).localOrigin.gameObject.name);
                        }
                    }

                    // OFFSETS
                    if (offsets != null)
                    {
                        for (int j = 0; j < offsets.Count; ++j)
                        {
                            description += "\n+ " + hand.side.ToString() + " hand offset " + offsets[j].distance.ToString() + " units in " + offsets[j].originDirection(hand).offsetDirection().ToString() + " direction based on origin transform: " + (offsets[j].originDirection(hand).localOrigin == null ? "INVALID" : offsets[j].originDirection(hand).localOrigin.gameObject.name);
                        }
                    }
                }

                // TWIST
                if (twist.HasValue)
                {
                    description += "\n+ Hand(s) twisting " + twist.Value.rotationDirection.ToString() + " by " + twist.Value.angle.ToString() + " degrees, with respect to hand " + twist.Value.rotationUpAxis + " as up axis";
                }

                // STILL
                if (still)
                {
                    description += "\n+ Hand(s) still";
                }

                // GRIPPING
                if (gripping.HasValue)
                {
                    description += "\n+ Hand(s) " + (gripping.Value ? "" : "not ") + "gripping";
                }

                // TRIGGERING
                if (triggering.HasValue)
                {
                    description += "\n+ Hand(s) " + (triggering.Value ? "" : "not ") + "pressing trigger";
                }

                // ALT USE
                if (altPressing.HasValue)
                {
                    description += "\n+ Hand(s) " + (altPressing.Value ? "" : "not ") + "pressing alt use";
                }

                // CASTING
                if (casting.HasValue)
                {
                    description += "\n+ Hand(s) " + (casting.Value ? "" : "not ") + "casting any spell";
                }

                // SPELL ACTIVE
                if (spellActive.HasValue)
                {
                    description += "\n+ " + spellActive.Value.spellID + " spell is " + (spellActive.Value.isActive ? "" : "not ") + "loaded on hand(s)";
                }

                // SPELL CASTING
                if (spellCasting.HasValue)
                {
                    description += "\n+ " + spellCasting.Value.spellID + " spell is " + (spellCasting.Value.isCasting ? "" : "not ") + "being cast with hand(s)";
                }

                // HOLDING ANYTHING
                if (holding.HasValue)
                {
                    description += "\n+ Hands(s) " + (holding.Value ? "" : "not ") + "holding anything"; 
                }

                // HOLDING ITEM
                if (holdingItem.HasValue)
                {
                    description += "\n+ Hands(s) " + (holdingItem.Value ? "" : "not ") + "holding an item";
                }

                // HOLDING ITEM TYPE
                if (holdingItemType.HasValue)
                {
                    description += "\n+ Hand(s) holding item of type " + holdingItemType.Value.ToString();
                }

                // HOLDING SPECIFIC ITEM
                if (!holdingItemSpecific_ID.IsNullOrEmptyOrWhitespace())
                {
                    description += "\n+ Hand(s) holding item " + holdingItemSpecific_ID;
                }

                // HOLDING CREATURE
                if (holdingCreature.HasValue)
                {
                    description += "\n+ Hand(s) " + (holdingCreature.Value ? "" : "not ") + "holding creature";
                }

                // HOLDING CREATURE TYPE
                if (holdingCreatureType.HasValue)
                {
                    description += "\n+ Hand(s) holding " + holdingCreatureType.Value.ToString() + " creature";
                }

                return description;
            }
        }

        /// <summary>
        /// Returns the condition(s) to check for the current gesture as a bool-returning Func
        /// </summary>
        public Func<bool> Condition
        {
            get
            {
                Func<bool> condition = new Func<bool>(() => true);

                // ABILITY LEVEL
                if (abilityLevelsRequired != null)
                {
                    foreach (KeyValuePair<VampireAbilityEnum, int> kvp in abilityLevelsRequired)
                    {
                        condition.AddCondition(new Func<bool>(() => VampireMaster.local.abilityLevels[kvp.Key] >= kvp.Value));
                    }
                }
                
                // HANDS
                for (int i = 0; i < hands.Length; ++i)
                {
                    RagdollHand currentHand = hands[i];
                    condition.AddCondition(new Func<bool>(() => currentHand != null));

                    // PART FACING
                    if (partFacing != null)
                    {
                        for (int j = 0; j < partFacing.Count; ++j)
                        {
                            (HandDirection handDirection, Func<RagdollHand, (Transform localOrigin, Func<Vector3> facingDirection)> originDirection) facing = partFacing[j];

                            // if facing direction vector is (0,0,0), the hand part can be facing any direction
                            if (facing.originDirection(currentHand).facingDirection() == Vector3.zero)
                            {
                                condition.AddCondition(new Func<bool>(() => true));
                                continue;
                            }

                            Func<Vector3> handDirection = GetHandDirectionVector(currentHand.side, facing.handDirection);

                            condition.AddCondition(new Func<bool>(() => Vector3.Angle(handDirection(), facing.originDirection(currentHand).facingDirection()) <= directionAngleTolerance));
                        }
                    }
                    
                    // MOVING
                    if (moving != null)
                    {
                        for (int j = 0; j < moving.Count; ++j)
                        {
                            (Func<RagdollHand, (Transform localOrigin, Func<Vector3> movingDirection)> originDirection, float magnitude) move = moving[j];

                            // if moving direction is (0,0,0) the hand can be moving in any direction
                            if (move.originDirection(currentHand).movingDirection() == Vector3.zero)
                            {
                                condition.AddCondition(new Func<bool>(() => currentHand.Velocity().magnitude >= move.magnitude));
                                continue;
                            }

                            condition.AddCondition(new Func<bool>(() => currentHand.Velocity().magnitude >= move.magnitude && Vector3.Angle(currentHand.Velocity(), move.originDirection(currentHand).movingDirection()) <= directionAngleTolerance));
                        }
                    }

                    // OFFSETS
                    if (offsets != null)
                    {
                        for (int j = 0; j < offsets.Count; ++j)
                        {
                            (Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)> originDirection, float distance) offset = offsets[j];

                            // if offset direction is (0,0,0) switch to check for distance apart
                            if (offset.originDirection(currentHand).offsetDirection() == Vector3.zero)
                            {
                                offset = (new Func<RagdollHand, (Transform localOrigin, Func<Vector3> offsetDirection)>(hand =>
                                {
                                    Transform origin = offset.originDirection(currentHand).localOrigin;
                                    Func<Vector3> newDirection = new Func<Vector3>(() => (hand.transform.position - origin.position).normalized);
                                    return (origin, newDirection);
                                }), offset.distance);
                                
                            }

                            condition.AddCondition(new Func<bool>(() =>
                            {
                                Vector3 handOffset = currentHand.transform.position - offset.originDirection(currentHand).localOrigin.position;
                                return Mathf.Cos(Vector3.Angle(handOffset, offset.originDirection(currentHand).offsetDirection())) * handOffset.magnitude > offset.distance;
                            }));

                        }
                    }
                    
                    // TWIST: TO-DO !!!
                    if (twist.HasValue)
                    {
                        (HandDirection rotationUpAxis, RotationDirection rotationDirection, float angle) twistCheck = twist.Value;

                        Func<Vector3> rotationUp = GetHandDirectionVector(currentHand.side, twistCheck.rotationUpAxis);
                    }

                    // STILL
                    if (still)
                    {
                        condition.AddCondition(new Func<bool>(() => currentHand.Velocity().magnitude <= 0.1f));
                    }

                    // GRIPPING
                    if (gripping.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => gripping.Value == PlayerControl.GetHand(currentHand.side).gripPressed));
                    }

                    // TRIGGERING
                    if (triggering.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => triggering.Value == PlayerControl.GetHand(currentHand.side).castPressed));
                    }

                    // ALT USE
                    if (altPressing.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => altPressing.Value == PlayerControl.GetHand(currentHand.side).alternateUsePressed));
                    }

                    // CASTING
                    if (casting.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => casting.Value == (VampireMaster.local.GetSpellCaster(currentHand.side).isFiring || VampireMaster.local.GetSpellCaster(currentHand.side).isMerging)));
                    }

                    // SPELL ACTIVE
                    if (spellActive.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => (spellActive.Value.spellID == VampireMaster.local.GetSpellCaster(currentHand.side).spellInstance.id) == spellActive.Value.isActive));
                    }

                    // SPELL CASTING
                    if (spellCasting.HasValue)
                    {
                        SpellCaster handCaster = VampireMaster.local.GetSpellCaster(currentHand.side);
                        condition.AddCondition(spellCasting.Value.isCasting ? new Func<bool>(() => handCaster.spellInstance.id == spellCasting.Value.spellID && (handCaster.isFiring || handCaster.isMerging)) : new Func<bool>(() => handCaster.spellInstance.id != spellCasting.Value.spellID || !(handCaster.isFiring || handCaster.isMerging)));
                    }

                    // HOLDING ANYTHING
                    if (holding.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => currentHand.grabbedHandle != null == holding.Value));
                    }

                    // HOLDING ITEM
                    if (holdingItem.HasValue)
                    {
                        condition.AddCondition(holdingItem.Value ? new Func<bool>(() => currentHand.grabbedHandle != null && currentHand.grabbedHandle.item != null) : new Func<bool>(() => currentHand.grabbedHandle == null || currentHand.grabbedHandle.item == null));
                    }

                    // HOLDING ITEM TYPE
                    if (holdingItemType.HasValue)
                    {
                        condition.AddCondition(new Func<bool>(() => currentHand.grabbedHandle != null && currentHand.grabbedHandle.item != null && currentHand.grabbedHandle.item.data.type == holdingItemType.Value));
                    }

                    // HOLDING SPECIFIC ITEM

                    // HOLDING CREATURE

                    // HOLDING CREATURE TYPE

                }


            }
        }

        public static implicit operator Tuple<string, Func<bool>>(ControlGesture gesture) => new Tuple<string, Func<bool>>(gesture.Description, gesture.Condition);

        //
        //
        //



        // OLD FUNCTIONS TO REPLACE
        public static Tuple<string, Func<bool>[]> HandTwist(HandType HandType, HandDirection handDirection, RotationDirection rotationDirection, float angle, Func<Vector3[]> anglesTurned)
        {
            switch (HandType)
            {
                case HandType.Left:
                    switch (handDirection)
                    {
                        case HandDirection.Palm:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => anglesTurned()[0].z < -angle  });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => anglesTurned()[0].z > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].z) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Palm RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Back:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => anglesTurned()[0].z > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => anglesTurned()[0].z < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].z) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Backhand RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Fingers:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => anglesTurned()[0].x < -angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => anglesTurned()[0].x > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].x) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Fingers RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Wrist:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => anglesTurned()[0].x > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => anglesTurned()[0].x < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].x) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Wrist RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Thumb:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => anglesTurned()[0].y > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => anglesTurned()[0].y < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].y) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Thumb RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.LittleFinger:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => anglesTurned()[0].y < -angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand Counter Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => anglesTurned()[0].y > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Hand to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].y) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Left Little Finger RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        default:
                            return new Tuple<string, Func<bool>[]>("Left HandTwist HandDirection Invalid", new Func<bool>[1] { () => false });
                    }
                case HandType.Right:
                    switch (handDirection)
                    {
                        case HandDirection.Palm:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => anglesTurned()[0].z < -angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => anglesTurned()[0].z > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Palm Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].z) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Palm RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Back:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => anglesTurned()[0].z > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => anglesTurned()[0].z < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].z) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Backhand RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Fingers:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => anglesTurned()[0].x < -angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => anglesTurned()[0].x > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].x) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Fingers RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Wrist:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => anglesTurned()[0].x > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => anglesTurned()[0].x < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].x) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Wrist RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Thumb:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => anglesTurned()[0].y > angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => anglesTurned()[0].y < -angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].y) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Thumb RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.LittleFinger:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => anglesTurned()[0].y < -angle });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand Counter Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => anglesTurned()[0].y > angle });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Hand to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[1] { () => Mathf.Abs(anglesTurned()[0].y) > angle });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Right Little Finger RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        default:
                            return new Tuple<string, Func<bool>[]>("Right HandTwist HandDirection Invalid", new Func<bool>[1] { () => false });
                    }
                case HandType.Both:
                    switch (handDirection)
                    {
                        case HandDirection.Palm:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].z < -angle,
                                        () => anglesTurned()[1].z < -angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands Counter Clockwise to " + angle.ToString() + " (Palm Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].z > angle,
                                        () => anglesTurned()[1].z > angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands to " + angle.ToString() + " (Palm Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].z) > angle,
                                        () => Mathf.Abs(anglesTurned()[1].z) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Palms RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Back:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].z > angle,
                                        () => anglesTurned()[1].z > angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Counter Clockwise to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].z < -angle,
                                        () => anglesTurned()[1].z < -angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand to " + angle.ToString() + " (Backhand Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].z) > angle,
                                        () => Mathf.Abs(anglesTurned()[1].z) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Backhand RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Fingers:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].x < -angle,
                                        () => anglesTurned()[1].x < -angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Counter Clockwise to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].x > angle,
                                        () => anglesTurned()[1].x > angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand to " + angle.ToString() + " (Fingers Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].x) > angle,
                                        () => Mathf.Abs(anglesTurned()[1].x) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Fingers RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Wrist:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].x > angle,
                                        () => anglesTurned()[1].x > angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Counter Clockwise to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].x < -angle,
                                        () => anglesTurned()[1].x < -angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand to " + angle.ToString() + " (Wrist Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].x) > angle,
                                        () => Mathf.Abs(anglesTurned()[1].x) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Wrist RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.Thumb:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].y > angle,
                                        () => anglesTurned()[1].y > angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand Counter Clockwise to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].y < -angle,
                                        () => anglesTurned()[1].y < -angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hand to " + angle.ToString() + " (Thumb Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].y) > angle,
                                        () => Mathf.Abs(anglesTurned()[1].y) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Thumb RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        case HandDirection.LittleFinger:
                            switch (rotationDirection)
                            {
                                case RotationDirection.Clockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].y < -angle,
                                        () => anglesTurned()[1].y < -angle
                                    });
                                case RotationDirection.CounterClockwise:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands Counter Clockwise to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[2]
                                    {
                                        () => anglesTurned()[0].y > angle,
                                        () => anglesTurned()[1].y > angle
                                    });
                                case RotationDirection.Either:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Hands to " + angle.ToString() + " (Little Finger Axis)", new Func<bool>[2]
                                    {
                                        () => Mathf.Abs(anglesTurned()[0].y) > angle,
                                        () => Mathf.Abs(anglesTurned()[0].y) > angle
                                    });
                                default:
                                    return new Tuple<string, Func<bool>[]>("Twist Both Little Finger RotationDirection Invalid", new Func<bool>[1] { () => false });
                            }
                        default:
                            return new Tuple<string, Func<bool>[]>("Right HandTwist HandDirection Invalid", new Func<bool>[1] { () => false });
                    }
                default:
                    return new Tuple<string, Func<bool>[]>("HandTwist HandType Invalid", new Func<bool>[1] { () => false });
            }

            //
        }

        public static Tuple<string, Func<bool>[]> HandsAngle(HandDirection handsDirection, ComparisonOperator comparisonOperator, float angleLimit, float angleTolerance = 10.0f)
        {
            string conditionName;
            Func<bool>[] angleCheck;

            Func<Vector3> leftHandDirection = GetHandDirectionVector(Side.Left, handsDirection);
            Func<Vector3> rightHandDirection = GetHandDirectionVector(Side.Right, handsDirection);

            switch (comparisonOperator)
            {
                case ComparisonOperator.Equal:
                    conditionName = handsDirection.ToString() + " On Each Hand " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Mathf.Abs(Vector3.Angle(leftHandDirection(), rightHandDirection()) - angleLimit) <= angleTolerance };
                    break;
                case ComparisonOperator.NotEqual:
                    conditionName = handsDirection.ToString() + "s On Each Hand Not " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Mathf.Abs(Vector3.Angle(leftHandDirection(), rightHandDirection()) - angleLimit) > angleTolerance };
                    break;
                case ComparisonOperator.LessThan:
                    conditionName = handsDirection.ToString() + "s On Each Hand Less Than " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Vector3.Angle(leftHandDirection(), rightHandDirection()) < angleLimit };
                    break;
                case ComparisonOperator.LessThanOrEqual:
                    conditionName = handsDirection.ToString() + "s On Each Hand Less Than " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Vector3.Angle(leftHandDirection(), rightHandDirection()) < angleLimit || Mathf.Abs(Vector3.Angle(leftHandDirection(), rightHandDirection()) - angleLimit) <= angleTolerance };
                    break;
                case ComparisonOperator.GreaterThan:
                    conditionName = handsDirection.ToString() + "s On Each Hand Greater Than " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Vector3.Angle(leftHandDirection(), rightHandDirection()) > angleLimit };
                    break;
                case ComparisonOperator.GreaterThanOrEqual:
                    conditionName = handsDirection.ToString() + "s On Each Hand Greater Than Or Equal To " + angleLimit.ToString() + " Degrees Apart";
                    angleCheck = new Func<bool>[1] { () => Vector3.Angle(leftHandDirection(), rightHandDirection()) > angleLimit || Mathf.Abs(Vector3.Angle(leftHandDirection(), rightHandDirection()) - angleLimit) <= angleTolerance };
                    break;
                default:
                    conditionName = "HandsAngle ComparisonOperator Invalid";
                    angleCheck = new Func<bool>[1] { () => false };
                    break;
            }

            return new Tuple<string, Func<bool>[]>(conditionName, angleCheck);

        }

        //=====================================================
        // UTILITY FUNCTIONS
        //=====================================================
        private static Func<Vector3> GetHandDirectionVector(Side side, HandDirection handDirection)
        {
            switch (handDirection)
            {
                case HandDirection.Palm:
                    return new Func<Vector3>(() => -VampireMaster.local.PlayerHand(side).transform.forward);
                case HandDirection.Back:
                    return new Func<Vector3>(() => VampireMaster.local.PlayerHand(side).transform.forward);
                case HandDirection.Fingers:
                    return new Func<Vector3>(() => -VampireMaster.local.PlayerHand(side).transform.right);
                case HandDirection.Wrist:
                    return new Func<Vector3>(() => VampireMaster.local.PlayerHand(side).transform.right);
                case HandDirection.Thumb:
                    if (side == Side.Left) return new Func<Vector3>(() => -VampireMaster.local.PlayerHand(Side.Left).transform.up);
                    else return new Func<Vector3>(() => VampireMaster.local.PlayerHand(Side.Right).transform.up);
                case HandDirection.LittleFinger:
                    if (side == Side.Left) return new Func<Vector3>(() => VampireMaster.local.PlayerHand(Side.Left).transform.up);
                    else return new Func<Vector3>(() => -VampireMaster.local.PlayerHand(Side.Right).transform.up);
                default:
                    return new Func<Vector3>(() => Vector3.zero);
            }
        }

        private Func<Vector3> GetGestureDirectionVector(Transform hand, Transform target, GestureDirection gestureDirection)
        {
            switch (gestureDirection)
            {
                case GestureDirection.Any:
                    Debug.LogError(debugKey + "GetGestureDirectionVector somehow used GestureDirection.Any");
                    return new Func<Vector3>(() => target.up);
                case GestureDirection.Forward:
                    return new Func<Vector3>(() => target.forward);
                case GestureDirection.Backward:
                    return new Func<Vector3>(() => -target.forward);
                case GestureDirection.Up:
                    return new Func<Vector3>(() => target.up);
                case GestureDirection.Down:
                    return new Func<Vector3>(() => -target.up);
                case GestureDirection.Left:
                    return new Func<Vector3>(() => -target.right);
                case GestureDirection.Right:
                    return new Func<Vector3>(() => target.right);
                case GestureDirection.Inside:
                    return new Func<Vector3>(() =>
                    {
                        Vector3 handLocalPos = target.InverseTransformPoint(hand.position);
                        if (handLocalPos.x == 0.0f) return -target.up;

                        if (handLocalPos.x < 0.0f) return target.right;
                        else return -target.right;
                    });
                case GestureDirection.Outside:
                    return new Func<Vector3>(() =>
                    {
                        Vector3 handLocalPos = target.InverseTransformPoint(hand.position);
                        if (handLocalPos.x == 0.0f) return -target.up;

                        if (handLocalPos.x < 0.0f) return -target.right;
                        else return target.right;
                    });
                case GestureDirection.Together:
                    return new Func<Vector3>(() => (target.position - hand.position).normalized);
                case GestureDirection.Apart:
                    return new Func<Vector3>(() => (hand.position - target.position).normalized);
                default:
                    Debug.LogError(debugKey + "GetGestureDirectionVector direction was invalid");
                    return new Func<Vector3>(() => target.up);
            }
        }

        private static Func<Creature[]> GetCreatures(CreatureType creatureTargetType)
        {
            switch (creatureTargetType)
            {
                case CreatureType.Player:
                    return new Func<Creature[]>(() => new Creature[1] { VampireMaster.local.playerCreature });
                case CreatureType.Thrall:
                    return new Func<Creature[]>(() => VampireMaster.local.vampireThralls.ToArray());
                case CreatureType.Other:
                    return new Func<Creature[]>(() =>
                    {
                        List<Creature> creatureList = new List<Creature>();
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (!creature.isPlayer && !VampireMaster.local.vampireThralls.Contains(creature))
                            {
                                creatureList.Add(creature);
                            }
                        }
                        return creatureList.ToArray();
                    });
                default:
                    return new Func<Creature[]>(() => null);
            }
        }

        /*
        private static Func<Item[]> GetItems(ItemType itemTargetType)
        {
            switch (itemTargetType)
            {
                case ItemType.Weapon:
                    return new Func<Item[]>(() =>
                    {
                        if (Item.allActive == null || Item.allActive.Count == 0) return null;
                        List<Item> itemList = new List<Item>();
                        foreach (Item item in Item.allActive)
                        {
                            if (item.data.tier > 0) itemList.Add(item);
                        }

                        if (itemList == null || itemList.Count == 0) return null;
                        return itemList.ToArray();
                    });
                case ItemType.Other:
                    return new Func<Item[]>(() =>
                    {
                        if (Item.allActive == null || Item.allActive.Count == 0) return null;
                        List<Item> itemList = new List<Item>();
                        foreach (Item item in Item.allActive)
                        {
                            if (item.data.tier <= 0) itemList.Add(item);
                        }

                        if (itemList == null || itemList.Count == 0) return null;
                        return itemList.ToArray();
                    });
                default:
                    return new Func<Item[]>(() => null);
            }
        }
        */
    }
}
