
using System;
using System.ComponentModel;
using HarmonyLib;
using UnityEngine;
using Player;
using System.Collections.Generic;
using LevelGeneration;
using SNetwork;




namespace ResourceHelper
{
    [HarmonyPatch(typeof(PlayerSyncModelData), nameof(PlayerSyncModelData.Setup))]
    internal static class Patch_PlayerSyncModelData
    {
        [HarmonyWrapSafe]
        public static void Postfix(PlayerSyncModelData __instance,PlayerAgent owner)
        {
            //Logs.Info(string.Format("OnStateChange {1}, {0}",__instance.gameObject.name,newState.status) );
            Res_Moniter moniter= owner.gameObject.AddComponent<Res_Moniter>();
            moniter.agent = owner;
            if(owner.IsLocallyOwned)
            {
                moniter.isLocal = true;
                Res_Moniter.localAgent = owner;
            }
           
        }
    }



    [HarmonyPatch(typeof(LG_PickupItem_Sync), nameof(LG_PickupItem_Sync.OnStateChange))]
    internal static class AttemptPickupInteraction
    {
        [HarmonyWrapSafe]
        public static void Postfix(LG_PickupItem_Sync __instance,pPickupItemState oldState, pPickupItemState newState)
        {
            //Logs.Info(string.Format("OnStateChange {1}, {0}",__instance.gameObject.name,newState.status) );
            int id = __instance.gameObject.GetInstanceID();
            if( Res_Manager.OBJ.ContainsKey(id) && newState.status == ePickupItemStatus.PickedUp)
            {
                //Logs.Info(string.Format("Pickup {0}",__instance.gameObject.name) );

                MarkUI_Manager.DestoryMarker(id);
                Res_Manager.RemoveID(id);
        
            }
        }
    }


    [HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.DoChangeState))]
    internal static class Patch_DoChangeState
    {
        
        [HarmonyWrapSafe]
        public static void Postfix(GameStateManager __instance,eGameStateName nextState)
        {

            if(nextState == eGameStateName.InLevel)
            {
                Res_Moniter.canUpdate = true;
            }
            else
            {
                Res_Moniter.canUpdate = false;
                if (nextState == eGameStateName.AfterLevel  || nextState == eGameStateName.ExpeditionFail  || nextState == eGameStateName.ExpeditionSuccess)
                {
                    Logs.Verbose(string.Format("Do Clear on state {0}",nextState));

                    MarkUI_Manager.DestoryMarkerAll();
                    Res_Manager.ClearDics();
                }
                else if(nextState == eGameStateName.ReadyToStopElevatorRide  || nextState == eGameStateName.StopElevatorRide)
                {
                    Res_Moniter.StoreList.Clear();
                    Res_Moniter.nextCheckPointReloadTime = 0f;
                    Res_Moniter.hasStore = false;
                    Res_Moniter.lastStoreDoor = Vector3.zero;
                }

            }
          
        }
    }

    [HarmonyPatch(typeof(SyncedNavMarkerWrapper), nameof(SyncedNavMarkerWrapper.OnStateChange))]
    internal static class Patch_Ping
    {
        [HarmonyWrapSafe]
        public static void Postfix(SyncedNavMarkerWrapper __instance, pNavMarkerState oldState, pNavMarkerState newState)
        {
            if (newState.status == eNavMarkerStatus.Visible)
		    {
                // Res_Manager.scan_markers_at_pos(newState.worldPos);
                Res_Moniter.PingList[newState.worldPos] = 0;
                Res_Moniter.PingListNextTime[newState.worldPos] = 0f;
                // Logs.Info("start ping tasks");

            }

        }
      
    }


    [HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.OnStateChange))]
    internal static class Patch_CheckpointManagerOnStateChange
    {
        [HarmonyWrapSafe]
        public static void Postfix(CheckpointManager __instance,pCheckpointState oldState, pCheckpointState newState, bool isRecall)
        {
            // Logs.Info(string.Format("oldstate {0} newstate {1}",oldState.lastInteraction,newState.lastInteraction));
            if (Res_Moniter.hasStore && newState.lastInteraction == eCheckpointInteractionType.ReloadCheckpoint)
            {
            
                Logs.Verbose("CheckpointManager prepare to reload");

                Res_Moniter.reloadcount = 0;
                Res_Moniter.reloadIndex = 0;
                Res_Moniter.nextCheckPointReloadTime = Clock.Time + 4f;
                
        
            }
            else if (newState.lastInteraction == eCheckpointInteractionType.StoreCheckpoint)
            {
                if(newState.doorLockPosition != Res_Moniter.lastStoreDoor )
                {
                    Logs.Verbose("CheckpointManager prepare to store ");
                    Res_Moniter.lastStoreDoor = newState.doorLockPosition;
                    Res_Moniter.hasStore = true;
                    Res_Moniter.StoreList.Clear();
                    foreach(KeyValuePair<int,PlaceNavMarkerOnGO> pair in MarkUI_Manager.MARKER)
                    {

                        GameObject obj = Res_Manager.OBJ[pair.Key];
                        if(obj != null)
                        {
                            Res_Moniter.StoreList.Add(obj.transform.position);
                        }

                    }
                    Res_Moniter.reloadIndex = Res_Moniter.StoreList.Count;
                    Logs.Verbose(string.Format("Store count {0}",Res_Moniter.StoreList.Count));
                }
               
            }

        }
      
    }

    [HarmonyPatch(typeof(FirstPersonItemHolder), nameof(FirstPersonItemHolder.UpdateItemTransition))]
    internal static class Patch_FPSIUpdateItemTransition
    {
        public static void Postfix(FirstPersonItemHolder __instance, float __result)
        {
            

            if(Res_Manager.value_aim_fade_factor < 1.0f)
            {
                float multi = __result;
                if(__instance.m_currentState == FPItemStateName.Aim && __instance.m_lastState == FPItemStateName.Hip) // Hip =》 Aim
                {
                    multi = 1f - __result;
                }
                else if(__instance.m_currentState == FPItemStateName.Hip && __instance.m_lastState == FPItemStateName.Aim)// Aim =》 Hip
                {
                    multi = __result;
                }
                else
                { 
                    multi = 1f;
                }

                if(multi < Res_Manager.value_aim_fade_factor)
                {
                    Res_Moniter.ads_alpha_multi = Res_Manager.value_aim_fade_factor;
                }
                else
                {
                    Res_Moniter.ads_alpha_multi = multi;
                }

                
            }
            else
            {
                Res_Moniter.ads_alpha_multi = 1.0f;
            }

          


        }
    }

}

