using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DockIQ.UI
{
    public sealed class LevelButtonView : MonoBehaviour
    {
        [SerializeField] private int _levelId = 1;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        public int LevelId => _levelId;
        public Button Button => _button;
        public TextMeshProUGUI Label => _label;
    }
}
