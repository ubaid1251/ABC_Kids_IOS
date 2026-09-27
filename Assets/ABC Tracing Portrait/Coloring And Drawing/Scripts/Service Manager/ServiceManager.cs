using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace KGF.Coloring
{
    public class ServiceManager : Singleton<ServiceManager>
    {
        public int SelectedPanel = 0;
       
        private ReferenceManager _referenceManager;


        

        public ReferenceManager referenceManager
        {
            get
            {
                if (_referenceManager == null)
                {
                    _referenceManager = FindObjectOfType<ReferenceManager>();

                    if (_referenceManager == null)
                    {
                        _referenceManager = new GameObject("Reference Manager").AddComponent<ReferenceManager>();
                    }
                }

                return _referenceManager;
            }
        }

        private SelectedCharacter _selectedCharacter;
        public SelectedCharacter selectedCharacter
        {
            get
            {
                if (_selectedCharacter == null)
                {
                    _selectedCharacter = FindObjectOfType<SelectedCharacter>();

                    if (_selectedCharacter == null)
                    {
                        _selectedCharacter = new GameObject("Selected Character").AddComponent<SelectedCharacter>();
                    }
                }

                return _selectedCharacter;
            }
        }

        
        #region Initialize-Service

        void InitializeInAppManager()
        {
            //if (inAppManager == null)
            //{
            //    GameObject obj = new GameObject();
            //    obj.name = "InAppManager";
            //    obj.transform.SetParent(instance.transform);
            //    inAppManager = obj.AddComponent<InAppManager>();
            //}
        }
        #endregion

        private void Awake()
        {
            if (instance == null)
                Debug.Log("Instance created of service manager");

            InitializeInAppManager();
        }
    }
}