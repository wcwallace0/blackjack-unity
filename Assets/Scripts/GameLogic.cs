using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : MonoBehaviour
{      
    // Each entry is an integer representing the value of its corresponding card
    // Aces are entered at 11. Upon receiving another card, if the player busts, aces will be 
    // counted as 1 as needed to prevent a bust (if possible).
    List<int> playerHand = new List<int>();
    private int playerValue; // Value of the player's hand
    private int wallet = 1000;
    private int wager = 0;

    // First value is the face down card
    List<int> dealerHand = new List<int>();
    private int dealerValue; // Value of the dealer's hand

    private GameObject flippedCard;
    private Vector2Int flippedCardValue;

    public GameObject hitButton;
    public GameObject standButton;

    public GameObject wagerInput;
    public GameObject wagerText;
    public TMP_Text walletText;

    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    private int cardOrder = 0;

    // Each new card will move to the position of this GameObject
    public GameObject dealerTarget;
    private Vector3 dealerTargetInitial;
    public GameObject playerTarget;
    private Vector3 playerTargetInitial;
    public float timeBetweenCards;

    public TMP_Text pValueText;
    public TMP_Text dValueText;

    public GameObject topOfDeck;

    private bool isStanding;

    public float winTime;
    public float alertTime;
    public GameObject alertPrefab;
    public Transform canvas;

    private void Start() {
        topOfDeck.GetComponent<SpriteRenderer>().sprite = MenuController.chosenCardBackSprite;
        dealerTargetInitial = dealerTarget.transform.position;
        playerTargetInitial = playerTarget.transform.position;
    }

    public void Deal() {
        bool success = int.TryParse(wagerInput.GetComponent<TMP_InputField>().text, out int input);
        if(!success) {
            StartCoroutine(DisplayAlert("ALERT", "YOUR WAGER MUST BE A NUMBER.", alertTime));
        } else if(input <= 0) {
            StartCoroutine(DisplayAlert("ALERT", "YOUR WAGER MUST BE MORE THAN $0.", alertTime));
        } else if(input > wallet) {
            StartCoroutine(DisplayAlert("ALERT", "YOU DON'T HAVE ENOUGH MONEY FOR THAT WAGER!", alertTime));
        } else {
            wager = input;
            // Disable input field
            wagerInput.SetActive(false);
            // Enable wager text
            wagerText.GetComponent<TMP_Text>().text = "CURRENT WAGER: $" + wager;
            wagerText.SetActive(true);

            // Enable value displays
            pValueText.enabled = true;
            dValueText.enabled = true;

            StartCoroutine(SetupGame());
        }
    }

    private IEnumerator SetupGame() {
        // Draw 2 cards for the dealer hand
        yield return AddCard(DrawCard(), dealerHand, false, true);
        yield return AddCard(DrawCard(), dealerHand, false, false);

        // Draw 2 cards for the player hand
        yield return AddCard(DrawCard(), playerHand, true, false);
        yield return AddCard(DrawCard(), playerHand, true, false);

        // Check if player hand or dealer hand is a blackjack
        if(playerValue == 21) {
            if(dealerValue == 21) {
                yield return FlipDealerCard();
                StartCoroutine(GameEnd("BOTH THE PLAYER AND THE DEALER GOT A BLACKJACK!", 0));
            } else {
                StartCoroutine(GameEnd("YOU GOT A BLACKJACK!", 1.5f));
            }
        } else if (dealerValue == 21) {
            yield return FlipDealerCard();
            StartCoroutine(GameEnd("THE DEALER GOT A BLACKJACK...", -1));
        } else {
            SetPlayerActions(true);
        }
    }

    // Returns an int representing a random card from the deck
    // 1 -> Ace
    // 2-10 -> 10
    // 11 -> Jack
    // 12 -> Queen
    // 13 -> King
    private Vector2Int DrawCard() {
        return new Vector2Int(Random.Range(1, 14), Random.Range(1, 5));
    }

    // Adds a new card to the specified hand
    // If new card causes the hand to bust, attempts to reduce aces
    // Displays card animation, moving from deck to hand
    private IEnumerator AddCard(Vector2Int card, List<int> hand, bool isPlayer, bool isFlipped) {
        // Instantiate new card object
        GameObject newCard;
        if(isPlayer) {
            newCard = Instantiate(cardPrefab, topOfDeck.transform.position, Quaternion.identity);
            newCard.GetComponent<Card>().target = playerTarget.transform.position;
            playerTarget.transform.Translate(new Vector3(0.7f, 0f, 0f));
        } else {
            newCard = Instantiate(cardPrefab, topOfDeck.transform.position, Quaternion.identity);
            newCard.GetComponent<Card>().target = dealerTarget.transform.position;
            dealerTarget.transform.Translate(new Vector3(0.7f, 0f, 0f));
        }

        // Give correct properties to new card
        Sprite sprite;
        if(isFlipped) {
            sprite = MenuController.chosenCardBackSprite;
            flippedCard = newCard;
            flippedCardValue = card;
        } else {
            sprite = cardSprites[13*(card.y-1) + card.x - 1];
        }

        SpriteRenderer sr = newCard.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = cardOrder;
        cardOrder++;

        int newValue = card.x;
        if(card.x == 1) {
            newValue = 11;
        } else if (card.x > 9) {
            newValue = 10;
        }

        hand.Add(newValue);
        if(isPlayer) {
            playerValue += newValue;
            if(playerValue > 21) {
                ReduceAces(playerHand, true);
            }

            // Update player value display
            pValueText.text = "PLAYER VALUE: " + playerValue;
        } else {
            dealerValue += newValue;
            if(dealerValue > 21) {
                ReduceAces(dealerHand, false);
            }

            // Update dealer value display
            if(isStanding) {
                dValueText.text = "DEALER VALUE: " + dealerValue;
            } else {
                dValueText.text = "DEALER VALUE: ?? + " + (dealerValue-dealerHand[0]);
            }
        }

        // Give some breathing room
        yield return new WaitForSeconds(timeBetweenCards);
    }

    // Attempts to bring the value of the given hand below 21 by
    // reducing any aces counted as 11 down to 1 (as needed)
    private void ReduceAces(List<int> hand, bool isPlayer) {
        for(int i = 0; i<hand.Count; i++) {
            if(hand[i] == 11 && hand.Sum() > 21) {
                hand[i] = 1;
            }
        }

        if(isPlayer) {
            playerValue = hand.Sum();
        } else {
            dealerValue = hand.Sum();
        }
    }

    // Called when the HIT button is pressed
    public void HitButton() {
        StartCoroutine(Hit());
    }

    private IEnumerator Hit() {
        SetPlayerActions(false);

        yield return AddCard(DrawCard(), playerHand, true, false);

        // Determine status of player
        if(playerValue > 21) {
            StartCoroutine(GameEnd("YOU BUSTED...", -1));
        } else {
            SetPlayerActions(true);
        }
    }

    // Called when the STAND button is pressed
    public void StandButton() {
        StartCoroutine(Stand());
    }

    private IEnumerator Stand() {
        SetPlayerActions(false);

        yield return new WaitForSeconds(1f);

        yield return FlipDealerCard();
        isStanding = true;

        // Draw cards until dealer beats player or busts
        while(dealerValue <= 16) {
            yield return AddCard(DrawCard(), dealerHand, false, false);
        }

        // Check which hand wins
        if(dealerValue > 21) {
            StartCoroutine(GameEnd("THE DEALER BUSTED!", 1));
        } else if(dealerValue > playerValue) {
            StartCoroutine(GameEnd("THE DEALER'S HAND BEAT YOURS...", -1));
        } else if(dealerValue < playerValue) {
            StartCoroutine(GameEnd("YOUR HAND BEAT THE DEALER'S!", 1));
        } else {
            StartCoroutine(GameEnd("YOUR HANDS MATCHED.", 0));
        }
    }


    // UI FUNCTIONS

    // Enable or disable Hit and Stand buttons
    private void SetPlayerActions(bool isActive) {
        hitButton.SetActive(isActive);
        standButton.SetActive(isActive);
    }

    private IEnumerator DisplayAlert(string header, string message, float displayTime) {
        // TODO
        GameObject alertBox = Instantiate(alertPrefab);
        alertBox.transform.SetParent(canvas, false);
        // get children
        TMP_Text[] children = alertBox.GetComponentsInChildren<TMP_Text>();
        children[0].text = header;
        children[1].text = message;

        yield return new WaitForSeconds(displayTime);
        
        Destroy(alertBox);
    }

    // Replaces the sprite of the dealer's first card with
    // the appropriate card sprite
    private IEnumerator FlipDealerCard() {
        Sprite newSprite = cardSprites[13*(flippedCardValue.y-1) + flippedCardValue.x - 1];
        flippedCard.GetComponent<SpriteRenderer>().sprite = newSprite;

        // Update dealer value display to include the flipped card
        dValueText.text = "DEALER VALUE: " + dealerValue;

        yield return new WaitForSeconds(2f);
    }

    // Display win message
    // Determine the amount to be given back to player
    // Update wallet display
    private IEnumerator GameEnd(string message, float multiplier) {
        // Clear out hands and values
        playerHand.Clear();
        dealerHand.Clear();
        playerValue = 0;
        dealerValue = 0;

        // Disable and clear value displays
        pValueText.text = "PLAYER VALUE: 0";
        dValueText.text = "DEALER VALUE: 0";
        pValueText.enabled = false;
        dValueText.enabled = false;

        // Reset card target positions
        dealerTarget.transform.position = dealerTargetInitial;
        playerTarget.transform.position = playerTargetInitial;

        // Clear all Cards from scene
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");
        foreach(GameObject card in cards) {
            Destroy(card);
        }

        isStanding = false;

        string header;
        if(multiplier > 0) {
            header = "WIN";
        } else if(multiplier < 0) {
            header = "LOSE";
        } else {
            header = "TIE";
        }
        StartCoroutine(DisplayAlert(header, message, winTime));
        yield return new WaitForSeconds(winTime);

        // Bring back wager input
        wagerInput.SetActive(true);
        wagerText.SetActive(false);

        // Update new wallet amount
        wallet += (int) (wager * multiplier);
        walletText.text = "Wallet: $" + wallet;
    }
}
