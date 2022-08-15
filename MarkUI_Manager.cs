
using UnityEngine;
using System.Collections.Generic;



namespace ResourceHelper
{
   internal static class MarkUI_Manager
    {
        public static Dictionary<int,PlaceNavMarkerOnGO> MARKER = new Dictionary<int,PlaceNavMarkerOnGO>();
        public static Dictionary<int,bool> VISIBLE=new Dictionary<int,bool>();
    
        public static Dictionary<int,float> START_FADE_TIME=new Dictionary<int,float>();
       
        public static void Create_ResMarker(int id)
        {
            if(Res_Manager.PID[id] != 0)
            {
                PlaceNavMarkerOnGO MarkerOnGO = new PlaceNavMarkerOnGO();
                MarkerOnGO.type = PlaceNavMarkerOnGO.eMarkerType.Locator;
                MarkerOnGO.m_nameToShow = Res_Manager.NAME[id];
                MarkerOnGO.m_marker = PlaceMarker(id, MarkerOnGO.m_nameToShow);
                
                MarkerOnGO.m_marker.SetTitle(MarkerOnGO.m_nameToShow);
                MarkerOnGO.m_marker.SetPlayerName(MarkerOnGO.m_nameToShow);
                MarkerOnGO.m_marker.SetColor(Res_Manager.RES_BNK[Res_Manager.PID[id]].color);
                MarkerOnGO.SetMarkerVisible(false);

                MARKER[id] = MarkerOnGO;
                VISIBLE[id] = false;
    
                START_FADE_TIME[id] = 0f;
                
                SetMarkerIcon(Res_Manager.PID[id],MarkerOnGO.m_marker);
                
            }
        }

        
        public static NavMarker PlaceMarker(int id, string name)
        {
            NavMarker navMarker = GuiManager.NavMarkerLayer.PrepareMarker(Res_Manager.OBJ[id], null);
            if (navMarker != null)
            {
                navMarker.SetVisualStates(NavMarkerOption.CarryItemTitleDistance, NavMarkerOption.CarryItemTitleDistance, NavMarkerOption.Empty, NavMarkerOption.Empty);
                return navMarker;
            }
            return null;
        }
        public static void SetMarkerIcon(uint persistentID, NavMarker marker)
        {
            string filename = Res_Manager.RES_BNK[persistentID].file_name;
            
            Sprite _sprite = null;
            if (!SpriteManager.TryGetSpriteCache(filename, 128.0f, out _sprite))
            {
                _sprite = SpriteManager.GenerateSprite(filename, 128.0f);
            }

            if (_sprite != null)
            {
                var renderer = marker.m_carryitem.GetComponentInChildren<SpriteRenderer>();
                renderer.sprite = _sprite;
            }

        }



        public static void DestoryMarker(int id)
        {
            if(MARKER.ContainsKey(id) )
            {
                if (MARKER[id].m_marker != null)
                {
                    GuiManager.NavMarkerLayer.RemoveMarker(MARKER[id].m_marker);
                    MARKER[id].m_marker = null;
                }
                UnityEngine.Object.Destroy(MARKER[id]);
            
                MARKER.Remove(id);
                VISIBLE.Remove(id);
        
                START_FADE_TIME.Remove(id);
            }

        }


        public static void DestoryMarkerAll()
        {
        
            foreach(KeyValuePair<int,PlaceNavMarkerOnGO> pair in MARKER)
            {
                if (pair.Value.m_marker != null)
                {
                    GuiManager.NavMarkerLayer.RemoveMarker(pair.Value.m_marker);
                    pair.Value.m_marker = null;
                }
                UnityEngine.Object.Destroy(pair.Value);

            }
            MARKER.Clear(); 
            VISIBLE.Clear(); 
   
            START_FADE_TIME.Clear(); 

        }

    }
    
}

