using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Acciones de los botones del menú principal y de la escena de opciones
public class MenuController : MonoBehaviour {

	public void Jugar() {
		SceneManager.LoadScene("Nivel1");
	}

	public void CargarNivel(int numero) {
		SceneManager.LoadScene("Nivel" + numero);
	}

	public void Opciones() {
		SceneManager.LoadScene("Opciones");
	}

	public void VolverAlMenu() {
		SceneManager.LoadScene("MenuPrincipal");
	}

	public void Salir() {
		Application.Quit();
	}
}
