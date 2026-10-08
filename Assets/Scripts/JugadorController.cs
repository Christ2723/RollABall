using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JugadorController : MonoBehaviour {

	//Declaro la variable de tipo RigidBody que luego asociaremos a nuestro Jugador
	private Rigidbody rb;

	//Inicializo el contador de coleccionables recogidos
	private int contador;

	//Total de coleccionables del nivel (se cuentan solos al iniciar)
	private int totalColeccionables;

	//Posición inicial para reaparecer si caemos o nos golpea un obstáculo
	private Vector3 posicionInicial;

	//Texto del contador
	public Text textoContador;

	//Controlador del nivel (timer, ganar, perder)
	public ControladorNivel controladorNivel;

	//Declaro la variable pública velocidad para poder modificarla desde la Inspector window
	public float velocidad = 10f;

	//Altura a partir de la cual consideramos que el jugador se cayó
	public float alturaCaida = -5f;

	// Use this for initialization
	void Start () {

		//Capturo esa variable al iniciar el juego
		rb = GetComponent<Rigidbody>();

		//Inicio el contador a 0
		contador = 0;

		//Cuento cuantos coleccionables hay en la escena
		totalColeccionables = GameObject.FindGameObjectsWithTag("Coleccionable").Length;

		posicionInicial = transform.position;

		//Actualizo el texto del contador por primera vez
		setTextoContador();
	}

	// Para que se sincronice con los frames de física del motor
	void FixedUpdate () {

		//Si el nivel terminó (ganó o perdió) el jugador ya no se mueve
		if (controladorNivel != null && controladorNivel.Terminado) return;

		//Estas variables nos capturan el movimiento en horizontal y vertical de nuestro teclado
		float movimientoH = Input.GetAxis("Horizontal");
		float movimientoV = Input.GetAxis("Vertical");

		//Un vector 3 es un trío de posiciones en el espacio XYZ, en este caso el que corresponde al movimiento
		Vector3 movimiento = new Vector3(movimientoH, 0.0f, movimientoV);

		//Asigno ese movimiento o desplazamiento a mi RigidBody, multiplicado por la velocidad que quiera darle
		rb.AddForce(movimiento * velocidad);
	}

	void Update () {

		//Si el jugador se cae de la plataforma vuelve al inicio
		if (transform.position.y < alturaCaida) {
			Reaparecer();
		}
	}

	//Se ejecuta al entrar a un objeto con la opción isTrigger seleccionada
	void OnTriggerEnter(Collider other) {

		if (other.gameObject.CompareTag("Coleccionable")) {

			//Desactivo el objeto
			other.gameObject.SetActive(false);

			//Incremento el contador en uno
			contador = contador + 1;

			//Actualizo el texto del contador
			setTextoContador();
		}
	}

	//Se ejecuta al chocar con un objeto sólido
	void OnCollisionEnter(Collision collision) {

		//Si choco con un obstáculo vuelvo a la posición inicial
		if (collision.gameObject.CompareTag("Obstaculo")) {
			Reaparecer();
		}
	}

	//Devuelve al jugador a su posición inicial y lo detiene
	void Reaparecer() {

		rb.linearVelocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		transform.position = posicionInicial;
	}

	//Actualizo el texto del contador (o aviso al controlador si las ha cogido todas)
	void setTextoContador() {

		textoContador.text = "Contador: " + contador.ToString() + " / " + totalColeccionables.ToString();

		if (contador >= totalColeccionables && controladorNivel != null) {
			controladorNivel.Ganar();
		}
	}
}
