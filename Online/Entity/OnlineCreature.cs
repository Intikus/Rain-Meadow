﻿using RWCustom;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RainMeadow
{
    public class OnlineCreature : OnlinePhysicalObject
    {
        public bool enteringShortCut;
        internal AbstractCreature creature => apo as AbstractCreature;
        internal Creature realizedCreature => apo.realizedObject as Creature;
        public AbstractCreature abstractCreature => apo as AbstractCreature;

        public OnlineCreature(OnlineCreatureDefinition def, AbstractCreature ac) : base(def, ac)
        {
            // ? anything special?
        }

        public static AbstractCreature AbstractCreatureFromString(World world, string creatureString)
        {
            string[] array = Regex.Split(creatureString, "<cA>");
            CreatureTemplate.Type type = new CreatureTemplate.Type(array[0], false);


            if (type.Index == -1)
            {
                RainMeadow.Debug("Unknown creature: " + array[0] + " creature not spawning");
                return null;
            }
            string[] array2 = array[2].Split(new char[]
            {
            '.'
            });



            EntityID id = EntityID.FromString(array[1]);

            int? num = BackwardsCompatibilityRemix.ParseRoomIndex(array2[0]);

            /*            if (RainMeadow.isArenaMode(out var _))
                        {

                            int num2 = world.GetAbstractRoom("arenasmallroom").index;
                            EntityID id2 = EntityID.FromString(array[1]);

                            WorldCoordinate denz = new WorldCoordinate(num2, -1, 1, int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture));

                            AbstractCreature abstractCreature2 = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(type), null, denz, id2);
                            abstractCreature2.state.alive = true;
                            abstractCreature2.setCustomFlags();

                            return abstractCreature2;

                        }*/

            foreach (var thing in array)
            {
                RainMeadow.Debug("ARRAY 1 " + thing);
            }

            foreach (var thing2 in array2)
            {
                RainMeadow.Debug("ARRAY TWO " + thing2);
            }

            if (num == null || !world.IsRoomInRegion(num.Value))
            {

                //  Slugcat<cA>ID.-1.1<cA>GATE_HI_CC.-1<cA>
                //  array[0] = "Slugcat"
                //  array[1] = "ID.-1.1"
                //  array[2] = "GATE_HI_CC.-1 

/*              [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY 1 Slugcat
                [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY 1 ID.- 1.0
                [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY 1 GATE_HI_CC.- 1
                [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY 1
                [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY TWO GATE_HI_CC
                [Info: RainMeadow] 22:37:45 | 11770 | OnlineCreature.AbstractCreatureFromString:ARRAY TWO -1*/
                num = world.GetAbstractRoom(array2[0]).index;  // Arena hates GATE_HI_CC. This is the default from the template



            }

            WorldCoordinate den = new WorldCoordinate(num.Value, -1, -1, int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture));
            AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(type), null, den, id);
            if (world != null)
            {
                abstractCreature.state.LoadFromString(Regex.Split(array[3], "<cB>"));
                if (abstractCreature.Room == null)
                {
                    RainMeadow.Debug(string.Concat(new string[]
                        {
                        "Spawn room does not exist: ",
                        array2[0],
                        " ~ ",
                        id.spawner.ToString(),
                        " creature not spawning"
                        }));
                    return null;
                }
                abstractCreature.setCustomFlags();
            }
            return abstractCreature;
        }


        
        // Maybe the abstract creature is not being seen correctly from here?
        public static OnlineEntity FromDefinition(OnlineCreatureDefinition newCreatureEvent, OnlineResource inResource)
        {
            World world = inResource is RoomSession rs ? rs.World : inResource is WorldSession ws ? ws.world : throw new InvalidProgrammerException("not room nor world");
            EntityID id = world.game.GetNewID();
            id.altSeed = newCreatureEvent.seed;

            RainMeadow.Debug("serializedObject: " + newCreatureEvent.serializedObject);
            AbstractCreature ac = AbstractCreatureFromString(world, newCreatureEvent.serializedObject);
            ac.ID = id;

            return new OnlineCreature(newCreatureEvent, ac);
        }

        protected override EntityState MakeState(uint tick, OnlineResource inResource)
        {
            return new AbstractCreatureState(this, inResource, tick);
        }

        public void RPCCreatureViolence(OnlinePhysicalObject onlineVillain, int? hitchunkIndex, PhysicalObject.Appendage.Pos hitappendage, Vector2? directionandmomentum, Creature.DamageType type, float damage, float stunbonus)
        {
            byte chunkIndex = (byte)(hitchunkIndex ?? 255);
            this.owner.InvokeRPC(this.CreatureViolence, onlineVillain, chunkIndex, hitappendage == null ? null : new AppendageRef(hitappendage), directionandmomentum, type, damage, stunbonus);
        }

        [RPCMethod]
        public void CreatureViolence(OnlinePhysicalObject? onlineVillain, byte victimChunkIndex, AppendageRef? victimAppendageRef, Vector2? directionAndMomentum, Creature.DamageType damageType, float damage, float stunBonus)
        {
            var victimAppendage = victimAppendageRef?.GetAppendagePos(this);
            var creature = (this.apo.realizedObject as Creature);
            if (creature == null) return;

            BodyChunk? hitChunk = victimChunkIndex < 255 ? creature.bodyChunks[victimChunkIndex] : null;
            creature.Violence(onlineVillain?.apo.realizedObject.firstChunk, directionAndMomentum, hitChunk, victimAppendage, damageType, damage, stunBonus);
        }

        public void ForceGrab(GraspRef graspRef)
        {
            var castShareability = new Creature.Grasp.Shareability(Creature.Grasp.Shareability.values.GetEntry(graspRef.Shareability));
            var other = graspRef.OnlineGrabbed.FindEntity(quiet: true) as OnlinePhysicalObject;
            if (other != null && other.apo.realizedObject != null)
            {
                var grabber = (Creature)this.apo.realizedObject;
                var grabbedThing = other.apo.realizedObject;
                var graspUsed = graspRef.GraspUsed;

                if (grabber.grasps[graspUsed] != null)
                {
                    if (grabber.grasps[graspUsed].grabbed == grabbedThing) return;
                    grabber.grasps[graspUsed].Release();
                }
                grabber.grasps[graspUsed] = new Creature.Grasp(grabber, grabbedThing, graspUsed, graspRef.ChunkGrabbed, castShareability, graspRef.Dominance, graspRef.Pacifying);
                grabbedThing.room = grabber.room;
                grabbedThing.Grabbed(grabber.grasps[graspUsed]);
                new AbstractPhysicalObject.CreatureGripStick(grabber.abstractCreature, grabbedThing.abstractPhysicalObject, graspUsed, graspRef.Pacifying || grabbedThing.TotalMass < grabber.TotalMass);
            }
        }

        public void BroadcastSuckedIntoShortCut(IntVector2 entrancePos, bool carriedByOther)
        {
            if (currentlyJoinedResource == null) return;
            foreach (var participant in currentlyJoinedResource.participants)
            {
                if (!participant.Key.isMe)
                {
                    participant.Key.InvokeRPC(this.SuckedIntoShortCut, entrancePos, carriedByOther);
                }
            }
        }

        [RPCMethod]
        public void SuckedIntoShortCut(IntVector2 entrancePos, bool carriedByOther)
        {
            enteringShortCut = true;
            var creature = (apo.realizedObject as Creature);
            var room = creature.room;
            creature?.SuckedIntoShortCut(entrancePos, carriedByOther);
            if (creature.graphicsModule != null)
            {
                Vector2 vector = room.MiddleOfTile(entrancePos) + Custom.IntVector2ToVector2(room.ShorcutEntranceHoleDirection(entrancePos)) * -5f;
                creature.graphicsModule.SuckedIntoShortCut(vector);
            }
            enteringShortCut = false;
        }

        public override string ToString()
        {
            return (this.apo as AbstractCreature).creatureTemplate.ToString() + base.ToString();
        }
    }
}
