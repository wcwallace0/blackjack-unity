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

    public Button hitButton;
    public Button standButton;

    public GameObject wagerInput;
    public GameObject wagerText;
    public TMP_Text walletText;

    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Sprite cardBack;
    private Vector3 cardSpawn = new Vector3(0f, 0f, 0f);
    private int cardOrder = 0;

    public void Deal() {
        bool success = int.TryParse(wagerInput.GetComponent<TMP_InputField>().text, out int input);
        if(!success) {
            DisplayAlert("Your wager must be a number.");
        } else if(input <= 0) {
            DisplayAlert("Your wager must be more than $0.");
        } else if(input > wallet) {
            DisplayAlert("You don't have enough money for that wager!");
        } else {
            wager = input;
            // Disable input field
            wagerInput.SetActive(false);
            // Enable wager text
            wagerText.GetComponent<TMP_Text>().text = "Current Wager: $" + wager;
            wagerText.SetActive(true);

            SetupGame();
        }
    }

    private void SetupGame() {
        // TODO
        // Collect wager from user input
        // Hide wager UI

        // Draw 2 cards for the dealer hand
        AddCard(DrawCard(), dealerHand, false, true);
        AddCard(DrawCard(), dealerHand, false, false);

        // Draw 2 cards for the player hand
        AddCard(DrawCard(), playerHand, true, false);
        AddCard(DrawCard(), playerHand, true, false);

        // Check if player hand or dealer hand is a blackjack
        if(playerValue == 21) {
            if(dealerValue == 21) {
                GameEnd("Tie! Both the player and the dealer got a blackjack!", 0);
            } else {
                GameEnd("You got a blackjack!", 1.5f);
            }
        } else if (dealerValue == 21) {
            GameEnd("The dealer got a blackjack...", -1);
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
    private void AddCard(Vector2Int card, List<int> hand, bool isPlayer, bool isFlipped) {
        // TODO Display add card animation

        Sprite sprite;
        if(isFlipped) {
            sprite = cardBack;
        } else {
            sprite = cardSprites[(card.x * card.y)-1];
        }

        // Instantiate new card object
        GameObject newCard = Instantiate(cardPrefab, cardSpawn, Quaternion.identity);
        SpriteRenderer sr = newCard.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = cardOrder;
        cardSpawn.x += 0.7f;
        cardOrder++;

        Debug.Log("adding new card " + card.x + " to hand");

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

            // TODO Update player value display
        } else {
            dealerValue += newValue;
            if(dealerValue > 21) {
                ReduceAces(dealerHand, false);
            }

            // TODO Update player value display
        }

        Debug.Log("new hands");
        Debug.Log("[" + string.Join(", ", playerHand) + "]");
        Debug.Log("[" + string.Join(", ", dealerHand) + "]");
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

    public void Hit() {
        SetPlayerActions(false);

        AddCard(DrawCard(), playerHand, true, false);

        // Determine status of player
        if(playerValue > 21) {
            GameEnd("Bust!", -1);
        } else {
            SetPlayerActions(true);
        }
    }

    public void Stand() {
        SetPlayerActions(false);

        // TODO
        // Flip over first dealer card

        // Draw cards until dealer beats player or busts
        while(dealerValue <= 16) {
            AddCard(DrawCard(), dealerHand, false, false);

            // TODO
            // Wait some time?
        }

        // Check which hand wins
        if(dealerValue > 21) {
            GameEnd("The dealer busted!", 1);
        } else if(dealerValue > playerValue) {
            GameEnd("Dealer wins!", -1);
        } else if(dealerValue < playerValue) {
            GameEnd("Player wins!", 1);
        } else {
            GameEnd("Tie!", 0);
        }
    }


    // UI FUNCTIONS

    // Enable or disable Hit and Stand buttons
    private void SetPlayerActions(bool isActive) {
        hitButton.interactable = isActive;
        standButton.interactable = isActive;
    }

    private void DisplayAlert(string message) {
        // TODO
        Debug.Log(message);
    }

    // Display win message
    // Determine the amount to be given back to player
    // Update wallet display
    private void GameEnd(string message, float multiplier) {
        DisplayAlert(message);
        wallet += (int) (wager * multiplier);
        walletText.text = "Wallet: $" + wallet;

        // Clear out hands and values
        playerHand.Clear();
        dealerHand.Clear();
        playerValue = 0;
        dealerValue = 0;

        // TODO display
        wagerInput.SetActive(true);
        wagerText.SetActive(false);
    }
}
