
using System;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using Player;
using System.Collections.Generic;

using BepInEx.Configuration;
using BepInEx;
using System.IO;
using System.Globalization;




namespace ResourceHelper
{
    internal static class Res_Manager
    {
        public static Dictionary<uint,Res_Data> RES_BNK=new Dictionary<uint,Res_Data>();
        public static Dictionary<int,GameObject> OBJ=new Dictionary<int,GameObject>();
        public static Dictionary<int,string> NAME=new Dictionary<int,string>();
        public static Dictionary<int,int> COUNT=new Dictionary<int,int>();
        public static Dictionary<int,uint> PID=new Dictionary<int,uint>();
        public static Dictionary<int,int> TAR=new Dictionary<int,int>();
        public static Dictionary<int,float> MAX_DIS=new Dictionary<int,float>();
        public static Dictionary<int,float> FORCE_SHOW_TIME=new Dictionary<int,float>();
        public static Dictionary<int,float> TIME_CHECK_VALID=new Dictionary<int,float>();
        
 
        public static float value_aim_fade_factor;
        public struct Res_Data
        {
            public string rundown_name;
            public string tag_name;
            public string default_name;
            public string file_name;
            public uint persistentID;
            public int ammoType;
            public int fade_dis;
            public UnityEngine.Color color;
            public ConfigEntry<string> config_name{get;set;}

        }

        public static bool InitializeResData(string file)
        {
            // Logs.Info(string.Format("Try InitializeResData file: {0}",file));
            string [] parts = file.Split(",");
            if(parts != null && parts.Length == 9)
            {
                try
                {
                    Res_Data data = new Res_Data();
                    
                    data.rundown_name = parts[0];
                    data.tag_name = parts[1];
                    data.persistentID = uint.Parse(parts[2],CultureInfo.InvariantCulture);
                    if(!RES_BNK.ContainsKey(data.persistentID))
                    {
                        data.default_name = parts[3];
                        data.ammoType = Math.Clamp(int.Parse(parts[4],CultureInfo.InvariantCulture),0,100);
                        data.fade_dis = Math.Clamp(int.Parse(parts[5],CultureInfo.InvariantCulture),1,59);
                        data.color = new UnityEngine.Color(float.Parse(parts[6], CultureInfo.InvariantCulture.NumberFormat),float.Parse(parts[7], CultureInfo.InvariantCulture.NumberFormat),float.Parse(parts[8], CultureInfo.InvariantCulture.NumberFormat),1.0f);
                        data.file_name = file.ToLower();

                        ConfigEntry<string> config;
                        CreateResConfig(data.rundown_name,data.tag_name,data.default_name,data.ammoType,out config);
                        data.config_name = config;

                        RES_BNK[data.persistentID] = data;
                        return true;
                    }
                    else
                    {
                        Logs.Error(string.Format("InitializePng duplicate persistentID: {0}",file));
                    }
                }
                catch(Exception error)
                {
                    Logs.Error(string.Format("InitializePng form error : {0} MSG:{1}",file,error.Message));
                }
           
            }
            else
            {
                Logs.Error(string.Format("InitializePng split failed : {0}",file));
            }
            return false;

            
        }
        public static void CreateResConfig(string rundown_name,string tag_name, string default_name,int ammoType, out ConfigEntry<string> config)
        {
           
            if(rundown_name == "ResourceHelper")
            {
                string type = ammoType == 20 ? "ResourceHelper - Resource Pack Name":"ResourceHelper - Item Name";

                tag_name = "Original Resource - " + tag_name;
                config = EntryPoint.config_path.Bind<string>(type, tag_name, default_name, "");

            }
            else
            {
                
                tag_name = rundown_name + " - " + tag_name;
                string desc = $"Extra resource from {rundown_name}" ;
                config = EntryPoint.config_path.Bind<string>(rundown_name, tag_name, default_name, desc);

            }

        }

        
        public static void InitializationID(int id)
        {
            OBJ[id] = null;
            COUNT[id] = 0;
            NAME[id] = "";
            PID[id] = 0;
            TAR[id] = 0;
            MAX_DIS[id] = 0f;
            FORCE_SHOW_TIME[id] = 0f;
            TIME_CHECK_VALID[id] = 0f;

        }


        public static void RemoveID(int id)
        {
            OBJ.Remove(id);
            COUNT.Remove(id);
            NAME.Remove(id);
            PID.Remove(id);
            TAR.Remove(TAR[id]);
            TAR.Remove(id);
            MAX_DIS.Remove(id);
            FORCE_SHOW_TIME.Remove(id);
            TIME_CHECK_VALID.Remove(id);
  


        }
        public static void ClearDics()
        {
            PID.Clear();
            TAR.Clear();
            PID.Clear();
            COUNT.Clear();
            OBJ.Clear();
            NAME.Clear();
            FORCE_SHOW_TIME.Clear();
            TIME_CHECK_VALID.Clear();
            Res_Moniter.PingList.Clear();
            Res_Moniter.PingListNextTime.Clear();
    
        }


        public static void Do_Man_Scan(PlayerAgent client,bool islocal)
        {
            RaycastHit[] hits;
            Vector3 pos;
            if(islocal)
            {
                pos = client.FPSCamera.Position;
                hits = Physics.SphereCastAll(pos, 2f, client.FPSCamera.Forward, 0.0f,LayerManager.MASK_PLAYER_INTERACT_SPHERE);

            }
            else
            {
                pos = client.transform.position;
                pos[1] += 1.0f;
                hits = Physics.SphereCastAll(pos, 1.5f, client.transform.forward, 0.5f,LayerManager.MASK_PLAYER_INTERACT_SPHERE);
            }
            
            for(int i=0;i<hits.Length;++i)
            {
                GameObject obj = hits[i].collider.transform.gameObject;
                if(!TAR.ContainsKey(obj.GetInstanceID()))
                {
                    if(Vector3.Distance(obj.transform.position,pos) < 1.5f || !Physics.Linecast(obj.transform.position,pos,LayerManager.MASK_WORLD))
                    {
                        // Logs.Info(string.Format("{1} hit {0}",obj.name,client.transform.name) );
                        TryAddItem(obj);

                    }
                    
                }
                
            }
        }


        public static bool Do_Pos_Scan(Vector3 pos)
        {
            RaycastHit[] hits;
            hits = Physics.SphereCastAll(pos, 2f, Vector3.up, 0.0f,LayerManager.MASK_PLAYER_INTERACT_SPHERE);

            for(int i=0;i<hits.Length;++i)
            {
                GameObject obj = hits[i].collider.transform.gameObject;
                if(!TAR.ContainsKey(obj.GetInstanceID()))
                {
                    if(Vector3.Distance(obj.transform.position,pos) < 1.5f || !Physics.Linecast(obj.transform.position,pos,LayerManager.MASK_WORLD))
                    {
                        // Logs.Info(string.Format("{1} hit {0}",obj.name,client.transform.name) );
                        if(TryAddItem(obj))
                        {
                            return true;
                        }
                    }
                }   
            }
            return false;
        }


        public static void GetMarkersAtPos(Vector3 pos)
        {
            RaycastHit[] hits = Physics.SphereCastAll(pos, 2.0f, Vector3.up, 0.0f,LayerManager.MASK_PLAYER_INTERACT_SPHERE);
            int id = 0;
            for(int i=0;i<hits.Length;++i)
            {
                GameObject obj = hits[i].collider.transform.gameObject;
                if(TAR.ContainsKey(obj.GetInstanceID()) )
                {
                    id = TAR[obj.GetInstanceID()];

                    if(id != 0 && MarkUI_Manager.MARKER.ContainsKey(id))
                    {
                        // Logs.Info(string.Format("get ping {0}",OBJ[id].name) );
                        FORCE_SHOW_TIME[id] = Clock.Time + 15f;
                    }
                }
                
            }

        }


        public static bool TryAddItem(GameObject hits)
        {
            
            Interact_Pickup_PickupItem inter = hits.GetComponent<Interact_Pickup_PickupItem>();
            if(inter == null)
            {
                TAR[hits.GetInstanceID()] = 0;//Add the ID to the dictionary and ignore the entity in the next detection.
                return false;
            }   
            if(!inter.enabled)
            {
                // Some items, such as Bulkhead Keys, can be detected when the box is not opened.
                return false;
            }
            int inter_id = hits.GetInstanceID();
            TAR[inter_id] = 0;

            
            LevelGeneration.LG_PickupItem_Sync item_syc = hits.GetComponentInParent<LevelGeneration.LG_PickupItem_Sync>();
            if(item_syc == null )
            {
                return false;
            }

            Item item = item_syc.item;
            if(item == null )
            {
                return false;
            }


            uint persistentID = item.ItemDataBlock.persistentID;
            if(!RES_BNK.ContainsKey(persistentID))
            {
                return false;
            }

            if(!item.ItemDataBlock.canMoveQuick)
            {
                return false;
            }


            GameObject obj = item_syc.gameObject;
            int id = obj.GetInstanceID();
            if(!OBJ.ContainsKey(id))
            {
                InitializationID(id);

                OBJ[id] = obj;
                PID[id] = persistentID;
                MAX_DIS[id] = RES_BNK[persistentID].fade_dis;
                COUNT[id] = Get_ResCount(RES_BNK[persistentID].ammoType, item_syc.m_stateReplicator.State.custom.ammo);
                if(COUNT[id] > 0)
                {
                    NAME[id] = RES_BNK[persistentID].config_name.Value +  " ×"  + COUNT[id].ToString(CultureInfo.InvariantCulture) ;
                }
                else
                {
                    NAME[id] = RES_BNK[persistentID].config_name.Value;
                }
                TIME_CHECK_VALID[id] = Clock.Time + 0.1f;

                // Logs.Info(string.Format("add ITEM {0},id{1}",obj.name,id) );
                MarkUI_Manager.Create_ResMarker(id);

                TAR[id] = inter_id;
                TAR[inter_id] = id;
                return true;
                
            }
            return false;
            
        }


        public static bool CheckVaild(int id)
        {
            GameObject obj = OBJ[id];

            LevelGeneration.LG_PickupItem_Sync item = obj.GetComponentInParent<LevelGeneration.LG_PickupItem_Sync>();
            if(item == null)
            {
                return false;
            }

            uint pid = item.item.ItemDataBlock.persistentID;
            if(PID[id] != pid)
            {
                return false;
            }

            if(COUNT[id] != Get_ResCount(RES_BNK[pid].ammoType, item.m_stateReplicator.State.custom.ammo))
            {
                return false;
            }
            

            return true;
        }
       
    

        public static int Get_ResCount(int ammoPerCount, float ammoBase)
        {
            if(ammoPerCount > 0)
            {
                return Mathf.FloorToInt(ammoBase / (ammoPerCount * 1.0f)) ;
            }
            return 0;
        }
    
    }
    

}

