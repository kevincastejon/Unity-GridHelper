using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RTS_Demo2
{
    public class GameUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private GameObject _moveDescription;
        [SerializeField] private GameObject _attackDescription;
        private PanelSlide _panelSlide;
        private PanelChar _panelChar;
        private PanelButtons _panelButtons;
        public Character CurrentCharacter
        {
            get
            {
                return _panelChar.Character;
            }

            set
            {
                _panelChar.Character = value;
            }
        }
        public bool IsMoveButtonClicked { get => _panelButtons.IsMoveButtonClicked; }
        public bool IsAttackButtonClicked { get => _panelButtons.IsAttackButtonClicked; }
        public bool IsSkipButtonClicked { get => _panelButtons.IsSkipButtonClicked; }
        private void Awake()
        {
            _panelSlide = GetComponentInChildren<PanelSlide>();
            _panelChar = GetComponentInChildren<PanelChar>();
            _panelButtons = GetComponentInChildren<PanelButtons>();
        }

        public void StartSlide(string title)
        {
            _panelSlide.StartSlide(title);
        }
        public void DoSlide(float progress)
        {
            _panelSlide.DoSlide(progress);
        }
        public void StopSlide()
        {
            _panelSlide.StopSlide();
        }

        public void StartInPanelChar()
        {
            _panelChar.StartInPanel();
        }
        public void DoInPanelChar(float progress)
        {
            _panelChar.DoInPanel(progress);
        }
        public void StopInPanelChar()
        {
            _panelChar.StopInPanel();
        }

        public void StartOutPanelChar()
        {
            _panelChar.StartOutPanel();
        }
        public void DoOutPanelChar(float progress)
        {
            _panelChar.DoOutPanel(progress);
        }
        public void StopOutPanelChar()
        {
            _panelChar.StopOutPanel();
        }

        public void StartOutInPanelChar()
        {
            _panelChar.StartOutInPanel();
        }
        public void DoOutInPanelChar(float progress)
        {
            _panelChar.DoOutInPanel(progress);
        }
        public void StopOutInPanelChar()
        {
            _panelChar.StopOutInPanel();
        }

        public void StartInPanelButtons()
        {
            _panelButtons.StartInPanel();
        }
        public void DoInPanelButtons(float progress)
        {
            _panelButtons.DoInPanel(progress);
        }
        public void StopInPanelButtons()
        {
            _panelButtons.StopInPanel();
        }

        public void StartOutPanelButtons()
        {
            _panelButtons.StartOutPanel();
        }
        public void DoOutPanelButtons(float progress)
        {
            _panelButtons.DoOutPanel( progress);
        }
        public void StopOutPanelButtons()
        {
            _panelButtons.StopOutPanel();
        }

        public void RefreshButtons()
        {
            _panelButtons.RefreshButtons(CurrentCharacter);
        }
        public void DisableAllButtons()
        {
            _panelButtons.DisableAllButtons();
        }
        public void SetTitle(string title)
        {
            _title.text = title;
        }

        public void ShowMoveDescription()
        {
            _moveDescription.SetActive(true);
            _attackDescription.SetActive(false);
        }
        public void ShowAttackDescription()
        {
            _moveDescription.SetActive(false);
            _attackDescription.SetActive(true);
        }
        public void HideDescriptions()
        {
            _moveDescription.SetActive(false);
            _attackDescription.SetActive(false);
        }
    }
}
