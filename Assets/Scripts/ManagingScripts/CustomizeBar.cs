using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class CustomizeBar : MonoBehaviour
{
    [SerializeField] private List<SkinSO> skins = new();
    [SerializeField] private Button selectButton;
    
    [Header("SkinVisuals")]
    [SerializeField] private Image skinImage;
    [SerializeField] private Image headImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI selectText;
    [SerializeField] private GameObject lockIcon;
    
    [Header("Buy")]
    [SerializeField] private TextMeshProUGUI shardsNum;
    [SerializeField] private TextMeshProUGUI skinCost;
    [SerializeField] private GameObject buyPopUp;
    [SerializeField] private Button yesButton;
    
    [Header("Unlock by level")]
    [SerializeField] private TextMeshProUGUI levelNeeded;
    [SerializeField] private GameObject levelUnlockPopUp;
    [SerializeField] private Button okButton;
    
    
    private int _currentSelection;
    private int _currentShards;

    void Start()
    {
        var currentSkinName = PlayerDataManager.Instance.GetCurrentSkin();
        _currentShards = PlayerDataManager.Instance.GetShardsNum();
        shardsNum.text = _currentShards.ToString();
        _currentSelection = skins.FindIndex(s => s.skinName == currentSkinName);

        if (_currentSelection == -1) _currentSelection = 0;
        
        UpdateVisual();
    }

    public void SwapSkin(int direction)
    {
        if (direction == 0)
        {
            _currentSelection++;

            if (_currentSelection >= skins.Count)
                _currentSelection = 0;

            UpdateVisual();
        }
        else if (direction == 1)
        { 
            if (_currentSelection == 0) 
                _currentSelection = skins.Count;

            _currentSelection--;
            
            UpdateVisual();
            
        }
    }

    private void UpdateVisual()
    {
        var selectedSkin = skins[_currentSelection];
        var owned = PlayerDataManager.Instance.IsSkinOwned(skins[_currentSelection].skinName);
        
        lockIcon.SetActive(!owned);
        
        
        if (owned)
        {
            var isSelected = (PlayerDataManager.Instance.GetCurrentSkin() == selectedSkin.skinName);
            selectText.text = isSelected ? "Selected" : "Select";
        }
        else
        {
            lockIcon.SetActive(true);
            selectText.text = "Unlock";
        }
        skinImage.sprite = skins[_currentSelection].skinSprite;
        backgroundImage.sprite = skins[_currentSelection].skinBg;
        headImage.sprite = skins[_currentSelection].head;
    }

    public void SelectSkin()
    {
        if (PlayerDataManager.Instance.IsSkinOwned(skins[_currentSelection].skinName))
        {
            PlayerDataManager.Instance.SetSkin(skins[_currentSelection].skinName);
            
            UpdateVisual();
        }
        else
        {
            if (skins[_currentSelection].isUnlockedThroughLevel)
            {
                levelUnlockPopUp.SetActive(true);
                levelNeeded.text = skins[_currentSelection].level.ToString();
                okButton.Select();
            }
            else
            {
                buyPopUp.SetActive(true);
                skinCost.text = skins[_currentSelection].costInShards.ToString();
                yesButton.Select();
            }
        }
    }

    public void ReturnToMenu() => SceneManager.LoadScene("Menu");

    public void ClosePopup(int id)
    {
        switch (id)
        {
            case 0:
                buyPopUp.SetActive(false);
                selectButton.Select();
                break;
            case 1:
                levelUnlockPopUp.SetActive(false);
                selectButton.Select();
                break;
        }
    }

    public void Buy()
    {
        var cost = skins[_currentSelection].costInShards;
        var skinName = skins[_currentSelection].skinName;
        if (_currentShards >= cost)
        {
            PlayerDataManager.Instance.Buy(cost);
            PlayerDataManager.Instance.BuySkin(skinName);
            PlayerDataManager.Instance.SetSkin(skinName);
            buyPopUp.SetActive(false);
            selectButton.Select();
            lockIcon.SetActive(false);
            selectText.text = "select";
        }
    }
}
