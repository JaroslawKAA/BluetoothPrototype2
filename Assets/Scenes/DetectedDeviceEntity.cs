using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Scenes
{
    public class DetectedDeviceEntity : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private Button connectButton;

        private void Awake()
        {
            Assert.IsNotNull(nameText);
            Assert.IsNotNull(addressText);
            Assert.IsNotNull(connectButton);
        }

        public void Init(string name, string mac)
        {
            nameText.text = name;
            addressText.text = mac;
            
            connectButton.onClick.AddListener(() =>
            {
                ConnectionTest.Instance.Connect(mac);
            });
        }
    }
}
