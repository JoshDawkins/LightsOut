using UnityEngine;

/*
//	Project Name: DawkinsJosh_LightsOut
//  Contribution: All code by Joshua Dawkins
//	Feature: Manages the state of a single window on the building
//	Start & End dates: 2/11/2021 - 2/11/2021
//	References: None
//	Links: https://trello.com/b/biXnpOpd/josh-dawkins-lights-out
//*/

[RequireComponent(typeof(Renderer))]
public class Window : MonoBehaviour
{
    [SerializeField]
    private int row = 0,
        column = 0;
    [SerializeField]
    private Material offMaterial = null,
        onMaterial = null;

    public int Row { get => row; }
    public int Column { get => column; }
	public bool IsOn { get; private set; }

    private new Renderer renderer;

	private void Start() {
        renderer = GetComponent<Renderer>();

        //Default state to off
        IsOn = false;
        renderer.material = offMaterial;
	}

	public void ToggleWindowLight() {
        //Flip the window's current IsOn state
        IsOn = !IsOn;

        //Change the material on the renderer
        renderer.material = IsOn ? onMaterial : offMaterial;
    }
}
