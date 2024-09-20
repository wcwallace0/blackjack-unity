using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public SpriteRenderer topOfDeck;
    private bool fadingOut = false;
    public float fadeTime = 2f;
    public PostProcessVolume blur;
    private DepthOfField dof;
    public GameObject menu;

    public GameObject cardBackMenu;
    public GameObject outline;
    public Sprite[] cardBackSprites;

    public static Sprite chosenCardBackSprite;

    public GameObject rulesMenu;

    private void Start() {
        dof = blur.profile.GetSetting<DepthOfField>();
        chosenCardBackSprite = cardBackSprites[0];
    }

    private void FixedUpdate() {
        if(fadingOut) {
            dof.focusDistance.value += Time.deltaTime*1.5f;
        }
    }

    // Fades out the buttons and blur effect, waits a little, then loads the Blackjack scene
    public void StartGame() {
        fadingOut = true;
        menu.SetActive(false);
        StartCoroutine(WaitForFade());
    }

    public void SetCardBackMenu(bool isActive) {
        cardBackMenu.SetActive(isActive);
    }

    public void SetCardBack(int cardChosen) {
        Sprite newSprite = cardBackSprites[cardChosen+1];
        topOfDeck.sprite = newSprite;
        chosenCardBackSprite = newSprite;
        outline.GetComponent<RectTransform>().anchoredPosition = new Vector3(cardChosen*96f, -35.6f, 0); // HORRIBLE
    }

    public void SetRulesMenu(bool isActive) {
        rulesMenu.SetActive(isActive);
    }

    public IEnumerator WaitForFade() {
        yield return new WaitForSeconds(fadeTime);

        SceneManager.LoadScene("Blackjack");
    }
}
