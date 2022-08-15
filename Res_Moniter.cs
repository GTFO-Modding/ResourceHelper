using BepInEx.IL2CPP.Utils.Collections;
using System.Collections;
using UnityEngine;
using System.Linq;
using Player;
using System.Collections.Generic;


namespace ResourceHelper
{

    public class  Res_Moniter : MonoBehaviour
    {
        public PlayerAgent agent = null;
        public bool isLocal = false;
        public float nextScanTime = 0f;

        public static PlayerAgent localAgent = null;
        public static bool canUpdate = false;
        public static bool hasStore = false;
        public static int reloadcount = 0;
        public static int reloadIndex = 0;
        public static float nextCheckPointReloadTime = 0f;
        public static Vector3 lastStoreDoor = Vector3.zero;
        public static Dictionary<Vector3,int> PingList=new Dictionary<Vector3,int>();
        public static Dictionary<Vector3,float> PingListNextTime=new Dictionary<Vector3,float>();
        public static List<Vector3> StoreList= new List<Vector3>();
        public static float ads_alpha_multi = 1f;

        public void LateUpdate()
        {
            if(canUpdate)
            {
                
                if(Clock.Time >= nextScanTime)
                {
                    nextScanTime = Clock.Time + 0.012f;
                    Res_Manager.Do_Man_Scan(agent,isLocal);
                }

                if(isLocal)
                {
                    LocalResMarkerUpdate();

                    CheckPingList();
                    CheckCheckPointReload();
                }
            }
        }

  
        public static void LocalResMarkerUpdate()
        {
            int target = 0;
            float min_ang = 360.0f;
            float min_dis = 100.0f;
            bool hasError = false;
            
            float nowtime = Clock.Time;


            foreach(KeyValuePair<int,PlaceNavMarkerOnGO> pair in MarkUI_Manager.MARKER)
            {
                int id = pair.Key;
                try
                {
                    GameObject obj = Res_Manager.OBJ[id];
                    float dis = Vector3.Distance(obj.transform.position, localAgent.FPSCamera.Position);

                    if(dis > 60f || (dis > Res_Manager.MAX_DIS[id] && Clock.Time > Res_Manager.FORCE_SHOW_TIME[id] ))
                    {
                        if(MarkUI_Manager.VISIBLE[id])
                        {
                            MarkUI_Manager.VISIBLE[id] = false;
                            pair.Value.SetMarkerVisible(false);
                        }
                        
                    }
                    else
                    {
                        //Check the item every 1 sec to prevent errors caused by conflicts with other plugins.
                        if(!TimerCheckValid(ref nowtime,ref id))
                        {
                            hasError = true;
                            target = id;
                            Logs.Error(string.Format("Item was changed, it's ok but why? ID:{0},Name:{1}",id,Res_Manager.NAME[id]));
                            break;
                        }


                        if(!MarkUI_Manager.VISIBLE[id])
                        {
                            MarkUI_Manager.VISIBLE[id] = true;
                            pair.Value.SetMarkerVisible(true);
                            MarkUI_Manager.START_FADE_TIME[id] = nowtime;
                        }
                        

                        // Set icon text
                        if(dis < 4f)// Force show name while distance < 4m
                        {
                            pair.Value.m_marker.SetTitle(pair.Value.m_nameToShow);
                        }
                        else
                        {
                            Vector3 pos = obj.transform.position;
                            pos[1] += dis * 0.015f; // When the distance is too far, the position of icon is higher than the actual position of the object.
                            float ang = Vector3.Angle((pos - localAgent.FPSCamera.Position).normalized, localAgent.FPSCamera.Forward);//Calculate the angle between the object and the camera
                            if(ang < ((-dis * 0.3) + 20.0))
                            {
                                if(ang < min_ang)
                                {
                                    min_ang = ang;
                                    min_dis = dis;
                                    target = id;
                                }
                                
                                pair.Value.m_marker.SetTitle(pair.Value.m_nameToShow);
                            }
                            else
                            {
                                pair.Value.m_marker.SetTitle("");
                            }

                        }

                        // Set icon alpha
                        SetMarkerAlpha(ref nowtime,ref id, pair.Value.m_marker);
                        
                    }
                    
                }
                catch
                {
                    hasError = true;
                    target = id;
                    Logs.Error(string.Format("Failed to check marker on id{0},Name:{1}",id,Res_Manager.NAME[id]));
                    break;
                    
                }

            }

            
            if(hasError)
            {
                GameObject obj = Res_Manager.OBJ[target];
                Res_Manager.RemoveID(target);
                MarkUI_Manager.DestoryMarker(target);
                Res_Manager.Do_Pos_Scan(obj.transform.position);
            }
            else
            {
                if(target != 0)
                {
                    // Zoom in on the icon closest to the crosshair.
                    float scale = MarkUI_Manager.MARKER[target].m_marker.transform.localScale[0] + min_dis * 0.003f;
                    MarkUI_Manager.MARKER[target].m_marker.transform.localScale = new Vector3(scale, scale, scale);

                }
            }

        }


        public static bool TimerCheckValid(ref float nowtime,ref int id)
        {
            if(nowtime > Res_Manager.TIME_CHECK_VALID[id])
            {
                if(Res_Manager.CheckVaild(id))
                {
                    Res_Manager.TIME_CHECK_VALID[id] = nowtime + 1.0f;
                    return true;
                }
                return false;
            }
            return true;
        }


        public static void SetMarkerAlpha(ref float nowtime,ref int id, NavMarker marker)
        {

            if(MarkUI_Manager.START_FADE_TIME[id] > 0f)
            {
                float percent = (nowtime - MarkUI_Manager.START_FADE_TIME[id]) / 0.1f;
                if(percent >= 0 && percent <= 1.0)
                {
                    float ease = Easing.GetEasingValue(eEasingType.EaseInSine, percent);
                    marker.SetAlpha(ease * ads_alpha_multi);
                }
                else
                {
                    MarkUI_Manager.START_FADE_TIME[id] = 0f;
                    marker.SetAlpha(ads_alpha_multi);
                }
            }
            else
            {
                marker.SetAlpha(ads_alpha_multi);
            }


        }



        public static void CheckPingList()
        {
            if(PingList.Count > 0)
            {
                
                List<Vector3> temlist = new List<Vector3>(PingList.Keys);
                Vector3 pos;
                for (int i = 0;i< temlist.Count;i++)
                {
                    pos = temlist[i];
                    if(Clock.Time > PingListNextTime[pos])
                    {
                        PingListNextTime[pos] = Clock.Time + 0.5f;
                        if(PingList[pos] < 7)
                        {
                            PingList[pos] += 1;
                            Res_Manager.GetMarkersAtPos(pos);
                        }
                        else
                        {
                            PingList.Remove(pos);
                            PingListNextTime.Remove(pos);
                        }
                    }
                    
                }

                // Logs.Error(string.Format("PingList.Count {0}",PingList.Count));
            }
           
        }


        public static void CheckCheckPointReload()
        {
            if(hasStore && nextCheckPointReloadTime > 0 && Clock.Time >nextCheckPointReloadTime)
            {
                if(reloadIndex < StoreList.Count)
                {
                    if(Res_Manager.Do_Pos_Scan(StoreList[reloadIndex]))
                    {
                        reloadcount ++;
                    }
                 
                    reloadIndex ++;
                }
                else
                {
                    nextCheckPointReloadTime = 0f;
                    if(reloadIndex == StoreList.Count)
                    {
                        Logs.Verbose(string.Format("Reload {0}/{1} store points",reloadcount,StoreList.Count));
                    }
                    
                }
            }
        }

       
    }

}