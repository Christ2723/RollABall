using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Controla el temporizador del nivel y qué pasa al ganar o perder
public class ControladorNivel : MonoBehaviour {

	//Segundos que tiene el jugador para recoger todo
	public float tiempoLimite = 60f;

	//Escena a la que se pasa al completar el nivel (vacío = es el último nivel)
	public string siguienteNivel = "";

	//Escena del menú principal
	public string escenaMenu = "MenuPrincipal";

	//Segundos que se muestra el letrero final antes de volver al menú
	public float esperaFinal = 5f;

	//Segundos que se muestra "Nivel completado" antes de pasar al siguiente
	public float esperaNivel = 3f;

	public Text textoTiempo, textoGanar;

	private float tiempoRestante;

	//Indica si el nivel ya terminó, para no ganar y perder a la vez
	public bool Terminado { get; private set; }

	void Start () {

		tiempoRestante = tiempoLimite;
		textoGanar.text = "";
		setTextoTiempo();
	}

	void Update () {

		//Con Escape volvemos al menú en cualquier momento
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SceneManager.LoadScene(escenaMenu);
			return;
		}

		if (Terminado) return;

		//Resto el tiempo que pasó desde el último frame
		tiempoRestante -= Time.deltaTime;

		if (tiempoRestante <= 0f) {
			tiempoRestante = 0f;
			Perder();
		}

		setTextoTiempo();
	}

	void setTextoTiempo() {

		textoTiempo.text = "Tiempo: " + Mathf.CeilToInt(tiempoRestante).ToString();

		//En los últimos 10 segundos el tiempo se pone rojo
		textoTiempo.color = tiempoRestante <= 10f ? Color.red : Color.white;
	}

	//La llama el JugadorController cuando recoge todos los coleccionables
	public void Ganar() {

		if (Terminado) return;
		Terminado = true;

		if (string.IsNullOrEmpty(siguienteNivel)) {
			//Último nivel: letrero grande y a los 5 segundos al menú
			textoGanar.color = new Color(1f, 0.85f, 0.1f);
			textoGanar.text = "¡Ganaste!";
			StartCoroutine(CargarEscena(escenaMenu, esperaFinal));
		} else {
			textoGanar.color = new Color(0.3f, 1f, 0.4f);
			textoGanar.text = "¡Nivel completado!";
			StartCoroutine(CargarEscena(siguienteNivel, esperaNivel));
		}
	}

	//Se acabó el tiempo
	void Perder() {

		Terminado = true;
		textoGanar.color = Color.red;
		textoGanar.text = "¡Perdiste!\n<size=60>Se acabó el tiempo</size>";
		StartCoroutine(CargarEscena(escenaMenu, esperaFinal));
	}

	IEnumerator CargarEscena(string escena, float espera) {

		yield return new WaitForSeconds(espera);
		SceneManager.LoadScene(escena);
	}
}
