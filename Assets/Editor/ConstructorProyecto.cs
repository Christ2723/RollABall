using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Construye automáticamente todas las escenas, materiales, prefabs y la música del juego.
//Se ejecuta desde el menú "Roll a Ball" de Unity o por línea de comandos.
public static class ConstructorProyecto {

	const string NOMBRE = "Christopher Payero Estrella";
	const string MATRICULA = "1-21-0909";

	static Material matFondo, matPared, matOro, matJugador, matObstaculo, matLaberinto, matPlataforma;
	static GameObject prefabCilindro, prefabCapsula;
	static AudioClip musica;
	static Font fuente;

	static readonly string[] escenas = {
		"MenuPrincipal", "Opciones", "Nivel1", "Nivel2", "Nivel3", "Nivel4", "Nivel5", "Nivel6"
	};

	[MenuItem("Roll a Ball/Construir todo")]
	public static void ConstruirTodo() {

		foreach (var carpeta in new[] { "Scenes", "Materiales", "Prefabs", "Audio" }) {
			if (!AssetDatabase.IsValidFolder("Assets/" + carpeta)) AssetDatabase.CreateFolder("Assets", carpeta);
		}

		AgregarTag("Coleccionable");
		AgregarTag("Obstaculo");

		fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		CrearMateriales();
		CrearMusica();
		CrearPrefabs();

		CrearMenuPrincipal();
		CrearOpciones();
		CrearNivel1();
		CrearNivel2();
		CrearNivel3();
		CrearNivel4();
		CrearNivel5();
		CrearNivel6();

		EditorBuildSettings.scenes = escenas
			.Select(e => new EditorBuildSettingsScene("Assets/Scenes/" + e + ".unity", true))
			.ToArray();

		PlayerSettings.companyName = NOMBRE;
		PlayerSettings.productName = "Roll a Ball";
		PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
		PlayerSettings.WebGL.decompressionFallback = false;

		AssetDatabase.SaveAssets();
		EditorSceneManager.OpenScene("Assets/Scenes/MenuPrincipal.unity");
		Debug.Log("ConstructorProyecto: todo construido");
	}

	[MenuItem("Roll a Ball/Compilar WebGL")]
	public static void CompilarWebGL() {

		var opciones = new BuildPlayerOptions {
			scenes = escenas.Select(e => "Assets/Scenes/" + e + ".unity").ToArray(),
			locationPathName = "Builds/WebGL",
			target = BuildTarget.WebGL,
			options = BuildOptions.None
		};
		var reporte = BuildPipeline.BuildPlayer(opciones);
		Debug.Log("ConstructorProyecto: build WebGL " + reporte.summary.result);
		if (reporte.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded && Application.isBatchMode) {
			EditorApplication.Exit(1);
		}
	}

	[MenuItem("Roll a Ball/Compilar Windows")]
	public static void CompilarWindows() {

		var opciones = new BuildPlayerOptions {
			scenes = escenas.Select(e => "Assets/Scenes/" + e + ".unity").ToArray(),
			locationPathName = "Builds/Windows/Roll a Ball.exe",
			target = BuildTarget.StandaloneWindows64,
			options = BuildOptions.None
		};
		var reporte = BuildPipeline.BuildPlayer(opciones);
		Debug.Log("ConstructorProyecto: build Windows " + reporte.summary.result);
	}

	// ---------------------------------------------------------------- Recursos

	static void AgregarTag(string tag) {

		var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		var tags = tagManager.FindProperty("tags");
		for (int i = 0; i < tags.arraySize; i++) {
			if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
		}
		tags.InsertArrayElementAtIndex(tags.arraySize);
		tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
		tagManager.ApplyModifiedProperties();
	}

	static Material CrearMaterial(string nombre, Color color, float metalico = 0f, float suavidad = 0.4f) {

		string ruta = "Assets/Materiales/" + nombre + ".mat";
		var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
		if (mat == null) {
			mat = new Material(Shader.Find("Standard"));
			AssetDatabase.CreateAsset(mat, ruta);
		}
		mat.color = color;
		mat.SetFloat("_Metallic", metalico);
		mat.SetFloat("_Glossiness", suavidad);
		EditorUtility.SetDirty(mat);
		return mat;
	}

	static void CrearMateriales() {

		matFondo = CrearMaterial("Fondo", new Color(0.13f, 0.2f, 0.45f));
		matPared = CrearMaterial("Pared", new Color(0.09f, 0.13f, 0.3f));
		matOro = CrearMaterial("Oro", new Color(1f, 0.8f, 0.1f), 0.6f, 0.7f);
		matJugador = CrearMaterial("Jugador", Color.white, 0f, 0.7f);
		matObstaculo = CrearMaterial("Obstaculo", new Color(0.85f, 0.15f, 0.15f), 0.2f, 0.5f);
		matLaberinto = CrearMaterial("Laberinto", new Color(0.35f, 0.2f, 0.55f));
		matPlataforma = CrearMaterial("Plataforma", new Color(0.15f, 0.45f, 0.4f));
	}

	//Genera una pista de música ambiental (acordes suaves + arpegio) y la guarda como WAV
	static void CrearMusica() {

		string ruta = "Assets/Audio/MusicaAmbiente.wav";
		if (!File.Exists(ruta)) {

			const int frecuencia = 22050;
			const float duracionAcorde = 4f;
			//La menor - Fa - Do - Sol, dos vueltas (32 segundos en bucle)
			float[][] acordes = {
				new[] { 220f, 261.63f, 329.63f },
				new[] { 174.61f, 220f, 261.63f },
				new[] { 261.63f, 329.63f, 392f },
				new[] { 196f, 246.94f, 293.66f },
			};
			int totalAcordes = 8;
			int muestras = (int)(frecuencia * duracionAcorde * totalAcordes);
			var datos = new float[muestras];

			for (int i = 0; i < muestras; i++) {
				float t = i / (float)frecuencia;
				int indice = (int)(t / duracionAcorde);
				float tAcorde = t - indice * duracionAcorde;
				float[] acorde = acordes[indice % acordes.Length];

				//Envolvente del pad: entra y sale suave para que no haya chasquidos
				float env = Mathf.Clamp01(tAcorde / 0.8f) * Mathf.Clamp01((duracionAcorde - tAcorde) / 0.8f);
				float pad = 0f;
				foreach (float f in acorde) {
					pad += Mathf.Sin(2 * Mathf.PI * f * t) + 0.6f * Mathf.Sin(2 * Mathf.PI * f * 1.004f * t);
				}
				pad += 0.8f * Mathf.Sin(2 * Mathf.PI * acorde[0] * 0.5f * t);
				pad *= 0.05f * env;

				//Arpegio tipo campana cada medio segundo
				float paso = 0.5f;
				int nota = (int)(tAcorde / paso);
				float tNota = tAcorde - nota * paso;
				int[] patron = { 0, 1, 2, 1, 0, 2, 1, 2 };
				float fNota = acorde[patron[nota % patron.Length]] * 2f;
				float campana = Mathf.Sin(2 * Mathf.PI * fNota * tNota) * Mathf.Exp(-tNota * 5f) * 0.12f;
				campana *= Mathf.Clamp01(tNota / 0.005f);

				datos[i] = Mathf.Clamp(pad + campana, -1f, 1f);
			}

			EscribirWav(ruta, datos, frecuencia);
			AssetDatabase.ImportAsset(ruta);
		}
		musica = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
	}

	static void EscribirWav(string ruta, float[] datos, int frecuencia) {

		using (var bw = new BinaryWriter(File.Create(ruta))) {
			int bytesDatos = datos.Length * 2;
			bw.Write("RIFF".ToCharArray());
			bw.Write(36 + bytesDatos);
			bw.Write("WAVE".ToCharArray());
			bw.Write("fmt ".ToCharArray());
			bw.Write(16);
			bw.Write((short)1);
			bw.Write((short)1);
			bw.Write(frecuencia);
			bw.Write(frecuencia * 2);
			bw.Write((short)2);
			bw.Write((short)16);
			bw.Write("data".ToCharArray());
			bw.Write(bytesDatos);
			foreach (float m in datos) bw.Write((short)(m * short.MaxValue));
		}
	}

	static GameObject CrearPrefabColeccionable(string nombre, PrimitiveType tipo, Vector3 escala) {

		var go = GameObject.CreatePrimitive(tipo);
		go.name = nombre;
		go.tag = "Coleccionable";
		go.transform.localScale = escala;
		go.transform.rotation = Quaternion.Euler(45, 45, 45);
		go.GetComponent<Renderer>().sharedMaterial = matOro;
		go.GetComponent<Collider>().isTrigger = true;

		//Rigidbody cinemático: objeto dinámico que no usa gravedad (apartado "Objetos estáticos y dinámicos")
		var rb = go.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = true;

		go.AddComponent<Rotador>();

		var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + nombre + ".prefab");
		UnityEngine.Object.DestroyImmediate(go);
		return prefab;
	}

	static void CrearPrefabs() {

		//Los coleccionables ahora son cilindros (y algunos cápsulas) en vez de cubos
		prefabCilindro = CrearPrefabColeccionable("Coleccionable", PrimitiveType.Cylinder, new Vector3(0.5f, 0.25f, 0.5f));
		prefabCapsula = CrearPrefabColeccionable("ColeccionableCapsula", PrimitiveType.Capsule, new Vector3(0.35f, 0.35f, 0.35f));
	}

	// ---------------------------------------------------------------- Piezas de escena

	static Scene NuevaEscena(Color? fondoCamara = null) {

		var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
		var luz = GameObject.Find("Directional Light");
		if (luz != null) luz.transform.rotation = Quaternion.Euler(50, 60, 0);
		if (fondoCamara.HasValue) {
			var cam = GameObject.Find("Main Camera").GetComponent<Camera>();
			cam.clearFlags = CameraClearFlags.SolidColor;
			cam.backgroundColor = fondoCamara.Value;
		}
		return escena;
	}

	static void Guardar(Scene escena, string nombre) {
		EditorSceneManager.SaveScene(escena, "Assets/Scenes/" + nombre + ".unity");
	}

	static GameObject CrearSuelo(float ancho, float largo) {

		var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
		suelo.name = "Suelo";
		suelo.transform.localScale = new Vector3(ancho / 10f, 1, largo / 10f);
		suelo.GetComponent<Renderer>().sharedMaterial = matFondo;
		return suelo;
	}

	static GameObject CrearCubo(Transform padre, string nombre, Vector3 posicion, Vector3 escala, Material mat) {

		var cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cubo.name = nombre;
		cubo.transform.SetParent(padre, false);
		cubo.transform.localPosition = posicion;
		cubo.transform.localScale = escala;
		cubo.GetComponent<Renderer>().sharedMaterial = mat;
		return cubo;
	}

	//Las 4 paredes del borde, igual que en el tutorial pero con tamaño variable
	static Transform CrearParedesPerimetro(float mitad) {

		var paredes = new GameObject("Paredes").transform;
		float largo = mitad * 2 + 0.5f;
		CrearCubo(paredes, "Pared Oeste", new Vector3(-mitad, 0, 0), new Vector3(0.5f, 2, largo), matPared);
		CrearCubo(paredes, "Pared Este", new Vector3(mitad, 0, 0), new Vector3(0.5f, 2, largo), matPared);
		CrearCubo(paredes, "Pared Norte", new Vector3(0, 0, mitad), new Vector3(largo, 2, 0.5f), matPared);
		CrearCubo(paredes, "Pared Sur", new Vector3(0, 0, -mitad), new Vector3(largo, 2, 0.5f), matPared);
		return paredes;
	}

	static GameObject CrearJugador(Vector3 posicion) {

		var jugador = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		jugador.name = "Jugador";
		jugador.transform.position = posicion;
		jugador.GetComponent<Renderer>().sharedMaterial = matJugador;
		var rb = jugador.AddComponent<Rigidbody>();
		rb.linearDamping = 0.3f;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		jugador.AddComponent<JugadorController>().velocidad = 10f;
		return jugador;
	}

	static void ConfigurarCamara(GameObject jugador, Vector3 offset) {

		var cam = GameObject.Find("Main Camera");
		cam.transform.position = jugador.transform.position + offset;
		cam.transform.LookAt(jugador.transform.position);
		cam.AddComponent<CamaraController>().jugador = jugador;
	}

	static Transform coleccionablesPadre;

	static void Coleccionable(Vector3 posicion, bool capsula = false) {

		if (coleccionablesPadre == null) coleccionablesPadre = new GameObject("Coleccionables").transform;
		var c = (GameObject)PrefabUtility.InstantiatePrefab(capsula ? prefabCapsula : prefabCilindro, coleccionablesPadre);
		c.transform.position = new Vector3(posicion.x, 0.5f, posicion.z);
	}

	static GameObject Obstaculo(string nombre, Vector3 posicion, Vector3 escala, Vector3 recorrido,
		float duracion, float desfase = 0f, float giro = 0f) {

		var o = CrearCubo(null, nombre, posicion, escala, matObstaculo);
		o.tag = "Obstaculo";
		var rb = o.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		var mov = o.AddComponent<ObstaculoMovil>();
		mov.recorrido = recorrido;
		mov.duracionCiclo = duracion;
		mov.desfase = desfase;
		mov.velocidadGiro = giro;
		return o;
	}

	static void CrearMusicaEnEscena() {

		var go = new GameObject("MusicaAmbiente");
		var audio = go.AddComponent<AudioSource>();
		audio.clip = musica;
		audio.loop = true;
		audio.playOnAwake = false;
		audio.volume = 0.5f;
		go.AddComponent<MusicaAmbiente>();
	}

	// ---------------------------------------------------------------- UI

	static Canvas CrearCanvas() {

		var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		var canvas = canvasGO.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		var scaler = canvasGO.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);
		scaler.matchWidthOrHeight = 0.5f;

		new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
		return canvas;
	}

	static Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, Vector2 ancla,
		Vector2 posicion, Vector2 dimension, TextAnchor alineacion, Color color, bool negrita = false) {

		var go = new GameObject(nombre, typeof(RectTransform), typeof(Text));
		go.transform.SetParent(padre, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = rt.pivot = ancla;
		rt.anchoredPosition = posicion;
		rt.sizeDelta = dimension;

		var t = go.GetComponent<Text>();
		t.font = fuente;
		t.text = contenido;
		t.fontSize = tamano;
		t.alignment = alineacion;
		t.color = color;
		t.fontStyle = negrita ? FontStyle.Bold : FontStyle.Normal;
		t.horizontalOverflow = HorizontalWrapMode.Overflow;
		t.verticalOverflow = VerticalWrapMode.Overflow;
		t.supportRichText = true;

		var sombra = go.AddComponent<Outline>();
		sombra.effectColor = new Color(0, 0, 0, 0.8f);
		sombra.effectDistance = new Vector2(2, -2);
		return t;
	}

	//Nombre y matrícula en la esquina superior derecha (en todas las escenas)
	static void CrearCreditos(Transform canvas) {

		CrearTexto(canvas, "TextoCreditos", NOMBRE + "\nMatrícula: " + MATRICULA, 30,
			new Vector2(1, 1), new Vector2(-25, -15), new Vector2(700, 90),
			TextAnchor.UpperRight, Color.white, true);
	}

	static Button CrearBoton(Transform padre, string texto, Vector2 posicion, Vector2 dimension, int tamano = 44) {

		var go = new GameObject("Boton" + texto.Replace(" ", ""), typeof(RectTransform), typeof(Image), typeof(Button));
		go.transform.SetParent(padre, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = posicion;
		rt.sizeDelta = dimension;

		var img = go.GetComponent<Image>();
		img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
		img.type = Image.Type.Sliced;
		img.color = Color.white;

		var boton = go.GetComponent<Button>();
		var colores = boton.colors;
		colores.normalColor = new Color(0.95f, 0.75f, 0.1f);
		colores.highlightedColor = new Color(1f, 0.88f, 0.4f);
		colores.pressedColor = new Color(0.75f, 0.55f, 0.05f);
		colores.selectedColor = colores.normalColor;
		boton.colors = colores;

		var t = CrearTexto(go.transform, "Texto", texto, tamano, new Vector2(0.5f, 0.5f), Vector2.zero,
			dimension, TextAnchor.MiddleCenter, new Color(0.1f, 0.1f, 0.2f), true);
		UnityEngine.Object.DestroyImmediate(t.GetComponent<Outline>());
		return boton;
	}

	static void FondoPantalla(Transform canvas, Color color) {

		var go = new GameObject("Fondo", typeof(RectTransform), typeof(Image));
		go.transform.SetParent(canvas, false);
		var rt = go.GetComponent<RectTransform>();
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = rt.offsetMax = Vector2.zero;
		go.GetComponent<Image>().color = color;
	}

	//HUD común de los niveles: contador, nivel, tiempo, créditos y letrero central
	static void CrearHUD(GameObject jugador, string titulo, float tiempo, string siguiente) {

		var canvas = CrearCanvas().transform;

		var textoContador = CrearTexto(canvas, "TextoContador", "Contador: 0", 40,
			new Vector2(0, 1), new Vector2(25, -15), new Vector2(500, 50), TextAnchor.UpperLeft, Color.white, true);
		CrearTexto(canvas, "TextoNivel", titulo, 30,
			new Vector2(0, 1), new Vector2(25, -65), new Vector2(600, 40), TextAnchor.UpperLeft, new Color(0.8f, 0.85f, 1f));
		var textoTiempo = CrearTexto(canvas, "TextoTiempo", "Tiempo: 60", 48,
			new Vector2(0.5f, 1), new Vector2(0, -15), new Vector2(400, 60), TextAnchor.UpperCenter, Color.white, true);
		CrearCreditos(canvas);

		//Texto de ganar: grande y en el centro de la pantalla
		var textoGanar = CrearTexto(canvas, "TextoGanar", "", 150,
			new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 500), TextAnchor.MiddleCenter, Color.yellow, true);
		var borde = textoGanar.GetComponent<Outline>();
		borde.effectDistance = new Vector2(5, -5);
		borde.effectColor = Color.black;

		var controlador = new GameObject("ControladorNivel").AddComponent<ControladorNivel>();
		controlador.tiempoLimite = tiempo;
		controlador.siguienteNivel = siguiente;
		controlador.textoTiempo = textoTiempo;
		controlador.textoGanar = textoGanar;

		var jc = jugador.GetComponent<JugadorController>();
		jc.textoContador = textoContador;
		jc.controladorNivel = controlador;

		CrearMusicaEnEscena();
	}

	// ---------------------------------------------------------------- Menús

	static void CrearMenuPrincipal() {

		var escena = NuevaEscena(new Color(0.05f, 0.07f, 0.15f));
		var canvas = CrearCanvas().transform;
		FondoPantalla(canvas, new Color(0.06f, 0.09f, 0.2f));
		var menu = new GameObject("Menu").AddComponent<MenuController>();

		CrearTexto(canvas, "Titulo", "ROLL A BALL", 140, new Vector2(0.5f, 0.5f), new Vector2(0, 330),
			new Vector2(1400, 180), TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.15f), true);
		CrearTexto(canvas, "Subtitulo", "Programación de Videojuegos", 40, new Vector2(0.5f, 0.5f), new Vector2(0, 220),
			new Vector2(1200, 60), TextAnchor.MiddleCenter, new Color(0.8f, 0.85f, 1f));

		var jugar = CrearBoton(canvas, "Jugar", new Vector2(0, 90), new Vector2(460, 100));
		UnityEventTools.AddPersistentListener(jugar.onClick, new UnityAction(menu.Jugar));

		var opciones = CrearBoton(canvas, "Opciones", new Vector2(0, -30), new Vector2(460, 100));
		UnityEventTools.AddPersistentListener(opciones.onClick, new UnityAction(menu.Opciones));

		var salir = CrearBoton(canvas, "Salir", new Vector2(0, -150), new Vector2(460, 100));
		UnityEventTools.AddPersistentListener(salir.onClick, new UnityAction(menu.Salir));

		CrearTexto(canvas, "TextoNiveles", "Elegir nivel:", 34, new Vector2(0.5f, 0.5f), new Vector2(0, -265),
			new Vector2(600, 50), TextAnchor.MiddleCenter, Color.white);
		for (int i = 1; i <= 6; i++) {
			var b = CrearBoton(canvas, i.ToString(), new Vector2(-325 + (i - 1) * 130, -345), new Vector2(110, 90), 48);
			UnityEventTools.AddIntPersistentListener(b.onClick, new UnityAction<int>(menu.CargarNivel), i);
		}

		CrearCreditos(canvas);
		CrearMusicaEnEscena();
		Guardar(escena, "MenuPrincipal");
	}

	static void CrearOpciones() {

		var escena = NuevaEscena(new Color(0.05f, 0.07f, 0.15f));
		var canvas = CrearCanvas().transform;
		FondoPantalla(canvas, new Color(0.06f, 0.09f, 0.2f));
		var menu = new GameObject("Menu").AddComponent<MenuController>();

		CrearTexto(canvas, "Titulo", "Opciones - Controles", 96, new Vector2(0.5f, 0.5f), new Vector2(0, 360),
			new Vector2(1400, 130), TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.15f), true);

		string controles =
			"<b>Flechas</b> o <b>W A S D</b>  →  Mover la bola\n" +
			"<b>Esc</b>  →  Volver al menú principal\n\n" +
			"<b>Objetivo:</b> recoge todos los coleccionables dorados\n" +
			"antes de que se acabe el tiempo.\n\n" +
			"• Si el tiempo llega a 0, pierdes.\n" +
			"• Si tocas un obstáculo rojo o te caes, vuelves al inicio.\n" +
			"• Hay 6 niveles; el 3 es un laberinto y el 4 tiene obstáculos que se mueven.";
		var t = CrearTexto(canvas, "TextoControles", controles, 40, new Vector2(0.5f, 0.5f), new Vector2(0, 20),
			new Vector2(1400, 520), TextAnchor.MiddleCenter, Color.white);
		t.lineSpacing = 1.15f;

		var volver = CrearBoton(canvas, "Volver", new Vector2(0, -360), new Vector2(460, 100));
		UnityEventTools.AddPersistentListener(volver.onClick, new UnityAction(menu.VolverAlMenu));

		CrearCreditos(canvas);
		CrearMusicaEnEscena();
		Guardar(escena, "Opciones");
	}

	// ---------------------------------------------------------------- Niveles

	static readonly Vector3 offsetCamara = new Vector3(0, 10, -10);

	//Nivel 1: el proyecto del tutorial con los cambios pedidos (cilindros, timer, etc.)
	static void CrearNivel1() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;
		CrearSuelo(20, 20);
		CrearParedesPerimetro(10);
		var jugador = CrearJugador(new Vector3(0, 0.5f, 0));
		ConfigurarCamara(jugador, offsetCamara);

		//12 coleccionables como las horas de un reloj
		for (int i = 0; i < 12; i++) {
			float angulo = i * Mathf.PI * 2f / 12f;
			Coleccionable(new Vector3(Mathf.Sin(angulo) * 7f, 0, Mathf.Cos(angulo) * 7f));
		}

		CrearHUD(jugador, "Nivel 1 - Clásico", 60, "Nivel2");
		Guardar(escena, "Nivel1");
	}

	//Nivel 2: arena más grande con muros interiores y coleccionables en líneas
	static void CrearNivel2() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;
		CrearSuelo(24, 24);
		var paredes = CrearParedesPerimetro(12);
		CrearCubo(paredes, "Muro Interior 1", new Vector3(-3, 0, 4), new Vector3(18, 2, 0.5f), matPared);
		CrearCubo(paredes, "Muro Interior 2", new Vector3(3, 0, -4), new Vector3(18, 2, 0.5f), matPared);

		var jugador = CrearJugador(new Vector3(-9, 0.5f, -9));
		ConfigurarCamara(jugador, offsetCamara);

		//Línea recta
		for (int i = 0; i < 5; i++) Coleccionable(new Vector3(-6 + i * 4, 0, -7));
		//Línea diagonal
		for (int i = 0; i < 5; i++) Coleccionable(new Vector3(-9 + i * 4.5f, 0, -1.8f + i * 0.9f), true);
		//Zigzag
		for (int i = 0; i < 7; i++) Coleccionable(new Vector3(-9 + i * 3, 0, i % 2 == 0 ? 6.5f : 9.5f));

		CrearHUD(jugador, "Nivel 2 - Líneas", 60, "Nivel3");
		Guardar(escena, "Nivel2");
	}

	//Nivel 3: laberinto
	static void CrearNivel3() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;

		string[] mapa = {
			"###############",
			"#P....#......C#",
			"#.###.#.#.###.#",
			"#.#...#.#...#C#",
			"#.#.###.#.#.###",
			"#.#.....#.#...#",
			"#.#######.###.#",
			"#C#.....#...#.#",
			"#.#.###.###.#.#",
			"#...#C#..C#.#.#",
			"#####.###.#.#.#",
			"#.....#...#...#",
			"#.#####.#####.#",
			"#C......#....C#",
			"###############",
		};
		const float celda = 2f;
		int n = mapa.Length;
		float centro = (n - 1) / 2f;

		var suelo = CrearSuelo(n * celda, n * celda);
		var laberinto = new GameObject("Laberinto").transform;
		Vector3 inicio = Vector3.zero;

		for (int fila = 0; fila < n; fila++) {
			if (mapa[fila].Length != n) throw new Exception("Fila del laberinto con largo incorrecto: " + fila);
			for (int col = 0; col < n; col++) {
				var pos = new Vector3((col - centro) * celda, 0, (centro - fila) * celda);
				switch (mapa[fila][col]) {
					case '#':
						CrearCubo(laberinto, "Muro", pos + Vector3.up * 0.75f, new Vector3(celda, 1.5f, celda), matLaberinto);
						break;
					case 'C':
						Coleccionable(pos, (fila + col) % 4 == 0);
						break;
					case 'P':
						inicio = pos + Vector3.up * 0.5f;
						break;
				}
			}
		}

		var jugador = CrearJugador(inicio);
		//Cámara más alta para ver por encima de los muros
		ConfigurarCamara(jugador, new Vector3(0, 13, -6));

		CrearHUD(jugador, "Nivel 3 - Laberinto", 90, "Nivel4");
		Guardar(escena, "Nivel3");
	}

	//Nivel 4: barras que se mueven de lado a lado
	static void CrearNivel4() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;
		CrearSuelo(24, 24);
		CrearParedesPerimetro(12);

		var jugador = CrearJugador(new Vector3(0, 0.5f, -10));
		ConfigurarCamara(jugador, offsetCamara);

		Obstaculo("Barra Movil 1", new Vector3(0, 0.5f, -4), new Vector3(7, 1, 0.8f), new Vector3(7, 0, 0), 3.5f);
		Obstaculo("Barra Movil 2", new Vector3(0, 0.5f, 0), new Vector3(7, 1, 0.8f), new Vector3(7, 0, 0), 2.8f, 0.5f);
		Obstaculo("Barra Movil 3", new Vector3(0, 0.5f, 4), new Vector3(7, 1, 0.8f), new Vector3(7, 0, 0), 3.2f, 0.25f);
		Obstaculo("Bloque Movil", new Vector3(0, 0.5f, 8), new Vector3(1.5f, 1, 1.5f), new Vector3(0, 0, 2.5f), 2f);

		foreach (float x in new[] { -8f, 0f, 8f }) {
			foreach (float z in new[] { -6f, -2f, 2f, 6f }) Coleccionable(new Vector3(x, 0, z), z > 0);
		}
		Coleccionable(new Vector3(-5, 0, 10));
		Coleccionable(new Vector3(5, 0, 10));

		CrearHUD(jugador, "Nivel 4 - Obstáculos móviles", 60, "Nivel5");
		Guardar(escena, "Nivel4");
	}

	//Nivel 5: plataformas flotantes y puentes estrechos (si te caes vuelves al inicio)
	static void CrearNivel5() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;
		var piso = new GameObject("Plataformas").transform;

		void Plataforma(string nombre, float x, float z, float ancho, float largo) {
			CrearCubo(piso, nombre, new Vector3(x, -0.25f, z), new Vector3(ancho, 0.5f, largo), matPlataforma);
		}

		Plataforma("Inicio", 0, -13, 6, 6);
		Plataforma("Puente Sur", 0, -7, 2, 6);
		Plataforma("Centro", 0, 0, 8, 8);
		Plataforma("Puente Este", 8.5f, 0, 9, 1.6f);
		Plataforma("Isla Este", 15, 0, 5, 5);
		Plataforma("Puente Oeste", -8.5f, 0, 9, 1.6f);
		Plataforma("Isla Oeste", -15, 0, 5, 5);
		Plataforma("Puente Norte 1", -2, 6, 1.6f, 4);
		Plataforma("Puente Norte 2", 0, 8.75f, 5.6f, 1.6f);
		Plataforma("Puente Norte 3", 2, 11.5f, 1.6f, 4);
		Plataforma("Isla Norte", 2, 16, 6, 6);

		var jugador = CrearJugador(new Vector3(0, 0.5f, -14));
		ConfigurarCamara(jugador, offsetCamara);

		for (int i = 0; i < 3; i++) Coleccionable(new Vector3(0, 0, -9 + i * 2));
		for (int i = 0; i < 3; i++) Coleccionable(new Vector3(6 + i * 2.5f, 0, 0), true);
		for (int i = 0; i < 3; i++) Coleccionable(new Vector3(-6 - i * 2.5f, 0, 0), true);
		Coleccionable(new Vector3(15, 0, 0));
		Coleccionable(new Vector3(-15, 0, 0));
		Coleccionable(new Vector3(-2, 0, 6));
		Coleccionable(new Vector3(2, 0, 11.5f));
		Coleccionable(new Vector3(0, 0, 16));
		Coleccionable(new Vector3(4, 0, 16));

		CrearHUD(jugador, "Nivel 5 - Plataformas", 60, "Nivel6");
		Guardar(escena, "Nivel5");
	}

	//Nivel 6: aspas giratorias + barra móvil + muros
	static void CrearNivel6() {

		var escena = NuevaEscena();
		coleccionablesPadre = null;
		CrearSuelo(26, 26);
		var paredes = CrearParedesPerimetro(13);
		CrearCubo(paredes, "Muro Centro", new Vector3(0, 0, 0), new Vector3(0.5f, 2, 8), matPared);
		CrearCubo(paredes, "Muro Norte Izq", new Vector3(-8, 0, 7), new Vector3(9, 2, 0.5f), matPared);
		CrearCubo(paredes, "Muro Norte Der", new Vector3(8, 0, 7), new Vector3(9, 2, 0.5f), matPared);

		var jugador = CrearJugador(new Vector3(0, 0.5f, -11));
		ConfigurarCamara(jugador, offsetCamara);

		Obstaculo("Aspa Izquierda", new Vector3(-6.5f, 0.5f, 0), new Vector3(8, 1, 0.6f), Vector3.zero, 1f, 0f, 90f);
		Obstaculo("Aspa Derecha", new Vector3(6.5f, 0.5f, 0), new Vector3(8, 1, 0.6f), Vector3.zero, 1f, 0f, -110f);
		Obstaculo("Barra Movil", new Vector3(0, 0.5f, -6), new Vector3(5, 1, 0.8f), new Vector3(8, 0, 0), 3f);
		Obstaculo("Bloque Norte", new Vector3(0, 0.5f, 10), new Vector3(1.5f, 1, 1.5f), new Vector3(9, 0, 0), 2.5f, 0.3f);

		//Anillos de coleccionables alrededor de cada aspa
		foreach (float cx in new[] { -6.5f, 6.5f }) {
			for (int i = 0; i < 6; i++) {
				float a = i * Mathf.PI * 2f / 6f;
				Coleccionable(new Vector3(cx + Mathf.Sin(a) * 3f, 0, Mathf.Cos(a) * 3f), i % 2 == 1);
			}
		}
		for (int i = 0; i < 5; i++) Coleccionable(new Vector3(-8 + i * 4, 0, 10.5f));

		CrearHUD(jugador, "Nivel 6 - Final", 60, "");
		Guardar(escena, "Nivel6");
	}
}
