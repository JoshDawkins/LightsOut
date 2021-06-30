using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
//	Project Name: DawkinsJosh_LightsOut
//  Contribution: All code by Joshua Dawkins
//	Feature: Manages the state and rules of the Lights Out game, including references to the windows, checking the game end conditions, and initially randomizing the board
//	Start & End dates: 2/11/2021 - 2/12/2021
//	References: None
//	Links: https://trello.com/b/biXnpOpd/josh-dawkins-lights-out
//*/

public class LoGameManager : MonoBehaviour
{
	//Psuedo-singleton instance
	public static LoGameManager Instance { get; private set; }

	[SerializeField]
	private GameObject difficultyPnl = null,
		winLbl = null,
		playAgainBtn = null;
	[SerializeField]
	private Text moveCountLbl = null;

	public bool GameEnabled { get; private set; }

	private Window[,] windows;//Two-dimensional array of all windows
	private int moveCount = 0;

	#region SETUP
	//Pseudo-singleton setup
	private void Awake() {
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	private void Start() {
		//Get all Window objects, sorted first by row index then column index
		var win = FindObjectsOfType<Window>().OrderBy(w => w.Row).ThenBy(w => w.Column);

		//Get a few key pieces of info we'll need to convert our flat collection into a 2D array
		int maxRow = win.Max(w => w.Row);
		int maxCol = win.Max(w => w.Column);

		//Initialize an empty array with the neccessary dimensions
		windows = new Window[maxRow + 1, maxCol + 1]; //Note that we add 1 to each here because we have max indices but the initializer requiers lengths

		//Finally, loop through our flat collection and insert each Window into its proper spot in the array
		foreach (Window w in win) {
			windows[w.Row, w.Column] = w;
		}

		//Disable the ability to click on windows initially
		GameEnabled = false;
	}
	#endregion SETUP

	#region GAMEPLAY
	private void Update() {
		//Check for click, if and only if the game is currently enabled
		if (GameEnabled && Input.GetMouseButtonDown(0)) {//0 = left click

			//Click received, make sure we aren't over a UI element, which has its own separate click handling
			if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
				return;//This means the pointer is over a UI element, so we can back off and just let it handle itself
			}

			//Otherwise, cast a ray from the camera to figure out what the player clicked on
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//This method handles defining a ray from the camera at the given point on the screen, in this
																		// case the mouse position

			//Cast the ray and see if it hits something, storing the hit result in the hit variable
			if (Physics.Raycast(ray, out RaycastHit hit)) {
				//We hit something; is it a window?
				Window window = hit.transform.GetComponent<Window>();
				if (window != null) {
					//It was a window, so toggle it and its adjacent windows
					ToggleGivenAndAdjacentWindows(window);

					//Increment move count and update display
					moveCount++;
					moveCountLbl.text = moveCount.ToString();

					//There is a chance the player's move may have won the game; let's check
					CheckForWin();
				}
			}
		}
	}

	//Called by the difficulty buttons to randomize the board and start the game
	public void BeginGame(int targetMoves) {
		//Hide the difficulty panel
		difficultyPnl.SetActive(false);

		//Get every possible window position
		List<Vector2Int> allPos = new List<Vector2Int>(windows.Length);
		foreach (Window w in windows)
			allPos.Add(new Vector2Int(w.Row, w.Column));

		//Simulate a number of unique but random moves equal to targetMoves (or the number of windows if it's less)
		//Since all windows are initially off, this toggles some of them on as if done by the player, guaranteeing a solvable puzzle
		for (int i = 0; i < targetMoves && allPos.Count > 0; i++) {
			Vector2Int ind = allPos[Random.Range(0, allPos.Count - 1)];
			ToggleGivenAndAdjacentWindows(windows[ind.x, ind.y]);
			allPos.Remove(ind);
		}

		//Enable the game!
		GameEnabled = true;
	}

	//Check for a win
	public void CheckForWin() {
		//The player wins if every window is off
		if (windows.OfType<Window>().All(w => !w.IsOn)) {//OfType filters an IEnumerable to only elements of a particular type, but more importantly here can be used to quickly flatten the 2D array into
														 //  a parameterized IEnumerable<> type, allowing us to use the other LINQ methods like .All()
			//WINNER!
			Debug.Log("All windows off, you win!");
			GameEnabled = false;
			winLbl.SetActive(true);
			playAgainBtn.SetActive(true);
		}
	}
	#endregion GAMEPLAY

	#region UTILITY
	//Utility method that toggles the given window, as well as any adjacent windows; can throw a NullReferenceException if there's a hole in the array
	private void ToggleGivenAndAdjacentWindows(Window window) {
		//Toggle the given window
		window.ToggleWindowLight();

		//Try to toggle the window above it
		if (window.Row > 0)//No window above if we're in row 0
			windows[window.Row - 1, window.Column].ToggleWindowLight();

		//Try to toggle the window below it
		if (window.Row < windows.GetLength(0) - 1)//No window below if we're in the last row (remember, length is one more than the last index)
			windows[window.Row + 1, window.Column].ToggleWindowLight();

		//Try to toggle the window to the left
		if (window.Column > 0)//No window left if we're in row 0
			windows[window.Row, window.Column - 1].ToggleWindowLight();

		//Try to toggle the window to the right
		if (window.Column < windows.GetLength(1) - 1)//No window right if we're in the last column (remember, length is one more than the last index)
			windows[window.Row, window.Column + 1].ToggleWindowLight();
	}

	//Called by the quit button
	public void QuitGame() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;//This will exit play mode in the editor
#else
         Application.Quit();//And this will actually close the game when built
#endif
	}

	//Called by the play again button
	public void ResetGame() {
		//Reload the scene
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	#endregion UTILITY
}
