using UnityEditor.Callbacks;
using UnityEditor;
using UnityEngine;
using DialogueGraph.Data;
using DialogueGraph.Editor.Views;

namespace DialogueGraph.Editor
{
    public class OnAssetOpen : MonoBehaviour
    {
        [OnOpenAsset]
        public static bool OpenGraphWindow(int instanceID, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is DialogueGraphSO so)
            {
                var window = EditorWindow.GetWindow<DialogueGraphEditorWindow>();
                window.LoadData(so);
                window.Show();
                return true; 
            }

            return false;
        }
    }
}
