/* Game.cs
 * Modified by: Jacob Grace
 * Author: Rod Howell
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ksu.Cis300.Klondike
{
    /// <summary>
    /// The game controller.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Keeps track of the currently selected discard pile
        /// </summary>
        private DiscardPile _discardPile;

        /// <summary>
        /// Keeps track of the currently selected tableau column
        /// </summary>
        private TableauColumn _selectedColumn;

        /// <summary>
        /// Keeps track of the amount of face down cards
        /// </summary>
        private int _faceDown = 21;

        /// <summary>
        /// Keeps track of the number of cards in the stock pile and discard pile together
        /// </summary>
        private int _stockCards = 24;

        /// <summary>
        /// The random number generator.
        /// </summary>
        private Random _randomNumbers = new Random();

        /// <summary>
        /// Gets a new card deck.
        /// </summary>
        /// <returns>The new card deck.</returns>
        private Card[] GetNewDeck()
        {
            Card[] cards = new Card[52];
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i] = new Card(i % 13 + 1, (Suit)(i / 13));
            }
            return cards;
        }

        /// <summary>
        /// Shuffles a new deck and pushes the cards onto the given stack.
        /// </summary>
        /// <param name="shuffled">The stack on which to push the cards.</param>
        private void ShuffleNewDeck(Stack<Card> shuffled)
        {
            Card[] deck = GetNewDeck();
            for (int i = deck.Length - 1; i >= 0; i--)
            {
                // Get a random nonnegative integer less than or equal to i.
                int j = _randomNumbers.Next(i + 1);

                shuffled.Push(deck[j]);
                deck[j] = deck[i];
            }
        }

        /// <summary>
        /// Constructs a new game from the given controls and seed.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="tableau">The tableau columns.</param>
        /// <param name="seed">The random number seed. If -1, no seed is used.</param>
        public Game(CardPile stock, TableauColumn[] tableau, int seed)
        {
            if(seed == -1)
            {
                seed = _randomNumbers.Next();
            }
            if (seed > -1)
            {
                seed = _randomNumbers.Next();
            }
            ShuffleNewDeck(stock.Pile);
            Deal(stock.Pile, tableau);
        }

        /// <summary>
        /// Draws the next three cards from the stock, or returns the discard pile to the stock
        /// if the stock is empty.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="discard">The discard pile.</param>
        public void DrawCardsFromStock(CardPile stock, DiscardPile discard)
        {

                if (stock.Pile.Count >= 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        discard.Pile.Push(stock.Pile.Pop());
                    }
                }
                else if(stock.Pile.Count == 0 && discard.Pile.Count > 0)
                {
                    for(int i = 0; i < discard.Pile.Count; i++)
                    {
                        stock.Pile.Push(discard.Pile.Pop());
                    }
                }
                else
                {
                    while(stock.Pile.Count > 0)
                    {
                        discard.Pile.Push(stock.Pile.Pop());

                    }
                }
        }

        /// <summary>
        /// Selects the top discarded card, or removes the selection if there already is one.
        /// </summary>
        /// <param name="discard">The discard pile.</param>
        public void SelectDiscard(DiscardPile discard)
        {
            if(_discardPile != null)
            {
                RemoveSelection();
            }
            else
            {
                _discardPile = discard;
                _discardPile.IsSelected = true;
            }
        }

        /// <summary>
        /// Selects the given number of cards from the given tableau column or tries to move
        /// any currently-selected cards to the given tableau column.
        /// </summary>
        /// <param name="col">The column to select or to move cards to.</param>
        /// <param name="n">The number of cards to select.</param>
        /// <returns>Whether the play wins the game.</returns>
        public bool SelectTableauCards(TableauColumn col, int n)
        {

            if (_discardPile != null)
            {
                if(n <= 1)
                {
                    MoveDiscardToTableau(col.FaceUpPile);
                }
                if (IsWon())
                {
                    return true;
                }
                RemoveSelection();
                return false;
            }


            else if (_selectedColumn != null)
            {

                if (n <= 1)
                {
                    MoveSelectionToTableau(col.FaceUpPile);
                }
                RemoveSelection();
                if(IsWon())
                {
                    return true;
                }
            }
            else
            {
                if(n >= 1)
                {
                    _selectedColumn = col;
                    _selectedColumn.NumberSelected = n;
                }
            }
            return false;

        }

        /// <summary>
        /// Moves the selected card to the given foundation pile, if possible
        /// </summary>
        /// <param name="dest">The foundation pile.</param>
        /// <returns>Whether the move wins the game.</returns>
        public bool SelectFoundationPile(Stack<Card> dest)
        {
            if(_discardPile != null)
            {
                MoveDiscardToFoundation(dest);
                RemoveSelection();
                if(IsWon())
                {
                    return true;
                }
            }
            else if(_selectedColumn != null)
            {
                MoveTableauToFoundation(dest);
                RemoveSelection();
                if(IsWon())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Method to transfer cards from one stack to another
        /// </summary>
        /// <param name="removed">Pile of cards to remove from</param>
        /// <param name="moved">Pile of cards to add to</param>
        /// <param name="num">Number of cards to move</param>
        private void TransferCards(Stack<Card> removed, Stack<Card> add, int num)
        {
            Card card;
            for (int i = 0; i < num; i++)
            {
                card = removed.Pop();
                add.Push(card);
            }
        }
        /// <summary>
        /// Removes the current selection of a tableau cell or discard pile
        /// </summary>
        private void RemoveSelection()
        {
            if (_discardPile != null)
            {
                _discardPile.IsSelected = false;
                _discardPile = null;

            }
            if (_selectedColumn != null)
            {
                _selectedColumn.NumberSelected = 0;
                _selectedColumn = null;

            }
        }
        /// <summary>
        /// Checks to see if the transfer of a card to a separate column is a valid move
        /// </summary>
        /// <param name="card">Card to be moved</param>
        /// <param name="stack">Column to be moved to</param>
        /// <returns></returns>
            private bool IsValidTableauMove(Card card, Stack<Card> stack)
            {
                if (stack.Count == 0)
                {
                    if(card.Rank == 13)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                Card topCard = stack.Peek();
                if(topCard.IsRed && card.IsRed || topCard.IsRed == false && card.IsRed == false)
                {
                    return false;
                }
                else
                {
                    if(topCard.Rank - 1 == card.Rank)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        /// <summary>
        /// Checks if a move to a foundation cell is a valid move
        /// </summary>
        /// <param name="card">Card to be moved</param>
        /// <param name="stack">Cell to be moved to</param>
        /// <returns></returns>
        private bool IsValidFoundationMove(Card card, Stack<Card> stack)
        {
            if(stack.Count == 0)
            {
                if(card.Rank == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            Card topCard = stack.Peek();
            if(card.Suit == topCard.Suit)
            {
                if(topCard.Rank + 1 == card.Rank)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Checks if the tableau column is empty and flips a face down card if there is one available
        /// </summary>
        /// <param name="column">Column to check</param>
        private void CheckEmptyTableau(TableauColumn column)
        {
            if(column.FaceUpPile.Count == 0 && column.FaceDownPile.Count > 0)
            {
                Card card = column.FaceDownPile.Pop();
                column.FaceUpPile.Push(card);
                _faceDown--;
            }
        }
        /// <summary>
        /// Deals the cards to start the game
        /// </summary>
        /// <param name="cards">Cards to deal</param>
        /// <param name="columns">Tableau Columns to deal to</param>
        private void Deal(Stack<Card> cards, TableauColumn[] columns)
        {
            for (int i = 1; i < 8; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    columns[i - 1].FaceDownPile.Push(cards.Pop());

                }

            }
            for (int i = 0; i < columns.Length; i++)
            {
                CheckEmptyTableau(columns[i]);
            }
        }
        /// <summary>
        /// Moves a tableau column card to another tableau column
        /// </summary>
        /// <param name="stack">Column for the card to be moved to</param>
        private void MoveSelectionToTableau(Stack<Card> stack)
        {
            Stack<Card> tempStack = new Stack<Card>();
            TransferCards(_selectedColumn.FaceUpPile, tempStack, _selectedColumn.NumberSelected);
            if(IsValidTableauMove(tempStack.Peek(),stack))
            { 
                TransferCards(tempStack, stack, _selectedColumn.NumberSelected);
                CheckEmptyTableau(_selectedColumn);
            }
            else
            {
                TransferCards(tempStack, _selectedColumn.FaceUpPile, _selectedColumn.NumberSelected);
            }
        }
        /// <summary>
        /// Moves a tableau column card to a Foundation cell
        /// </summary>
        /// <param name="stack">Foundation cell to move the card to</param>
        private void MoveTableauToFoundation(Stack<Card> stack)
        {
            if(_selectedColumn.NumberSelected > 1)
            {
                throw new InvalidOperationException();
            }
            else if(_selectedColumn.NumberSelected == 1)
            {
                if(IsValidFoundationMove(_selectedColumn.FaceUpPile.Peek(), stack))
                {
                    Card card = _selectedColumn.FaceUpPile.Pop();
                    stack.Push(card);
                    CheckEmptyTableau(_selectedColumn);
                    RemoveSelection();
                }
            }
        }
        /// <summary>
        /// Checks to see if the game is won
        /// </summary>
        /// <returns>True or false depending on if the game has been won</returns>
        private bool IsWon()
        {
            if(_stockCards <=1 && _faceDown == 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Moves a card from the discard pile to a tableau column
        /// </summary>
        /// <param name="cards">Pile to move the card to</param>
        private void MoveDiscardToTableau(Stack<Card> cards)
        {
            if(_discardPile != null)
            {
                if (IsValidTableauMove(_discardPile.Pile.Peek(), cards))
                {
                    cards.Push(_discardPile.Pile.Pop());
                    _stockCards--;
                }
            }
           
        }
        /// <summary>
        /// Moves a card from the discard pile to the foundation piles
        /// </summary>
        /// <param name="cards">Foundation pile to move to</param>
        private void MoveDiscardToFoundation(Stack<Card> cards)
        {
            if(_discardPile != null)
            {
                if (IsValidFoundationMove(_discardPile.Pile.Peek(), cards))
                {
                    cards.Push(_discardPile.Pile.Pop());
                    _stockCards--;
                }
            }
        }



    }
}
