using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class PelotaColisiones : MonoBehaviour
{
    const float PixelesPorUnidad = 100f;
    const float Radio = 0.24f;

    [SerializeField] RectTransform galeria;
    [SerializeField] RectTransform[] botones;
    [SerializeField] TMP_Text[] textos;
    [SerializeField] Image[] iconos;

    readonly BoxCollider2D[] limites = new BoxCollider2D[4];
    readonly Vector3[] esquinas = new Vector3[4];
    RectTransform canvas;
    GameObject escenarioFisico;
    BoxCollider2D[] obstaculos;
    BoxCollider2D[] cajasTexto;
    PolygonCollider2D[] siluetas;
    Rect[] rectangulosIconos;
    Rigidbody2D cuerpo;
    CircleCollider2D circulo;
    Image imagen;
    Texture2D textura;
    Sprite sprite;
    PhysicsMaterial2D material;
    Vector2 pantallaAnterior;

    void Start()
    {
        canvas = (RectTransform)transform;
        material = new PhysicsMaterial2D("Rebote de pelota") { friction = 0, bounciness = 0.76f };

        escenarioFisico = new GameObject("Colisiones del menú");
        var objeto = new GameObject("Pelota (física)");
        objeto.transform.SetParent(escenarioFisico.transform, false);
        cuerpo = objeto.AddComponent<Rigidbody2D>();
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        cuerpo.freezeRotation = true;
        circulo = objeto.AddComponent<CircleCollider2D>();
        circulo.radius = Radio;
        circulo.sharedMaterial = material;

        string[] nombres = { "Piso", "Techo", "Pared izquierda", "Pared derecha" };
        for (int i = 0; i < limites.Length; i++)
            limites[i] = CrearCaja(nombres[i]);
        obstaculos = new BoxCollider2D[botones.Length];
        for (int i = 0; i < botones.Length; i++)
            obstaculos[i] = CrearCaja("Colisión: " + botones[i].name);

        cajasTexto = new BoxCollider2D[textos.Length];
        for (int i = 0; i < textos.Length; i++)
            cajasTexto[i] = CrearCaja(textos[i].name);

        siluetas = new PolygonCollider2D[iconos.Length];
        rectangulosIconos = new Rect[iconos.Length];
        for (int i = 0; i < iconos.Length; i++)
        {
            var objetoIcono = new GameObject(iconos[i].name);
            objetoIcono.transform.SetParent(escenarioFisico.transform, false);
            siluetas[i] = objetoIcono.AddComponent<PolygonCollider2D>();
            siluetas[i].sharedMaterial = material;
        }

        imagen = new GameObject("Pelota (imagen)", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        imagen.rectTransform.SetParent(canvas, false);
        imagen.raycastTarget = false;
        imagen.sprite = CrearSprite();
        imagen.color = new Color(0.22f, 0.9f, 1f);

        Canvas.ForceUpdateCanvases();
        Vector2 tamano = ActualizarCajas();
        cuerpo.position = new Vector2(tamano.x * 0.22f, tamano.y * 0.84f);
        pantallaAnterior = tamano;
    }

    void Update()
    {
        bool visible = !galeria.gameObject.activeInHierarchy;
        imagen.enabled = visible;
        cuerpo.simulated = visible;
        var raton = Mouse.current;
        if (!visible || raton == null || !raton.leftButton.wasPressedThisFrame)
            return;

        Vector2 clic = raton.position.ReadValue() / PixelesPorUnidad;
        if ((clic - (Vector2)cuerpo.transform.position).sqrMagnitude > Radio * Radio)
            return;

        float haciaElCentro = cuerpo.position.x < pantallaAnterior.x * 0.5f ? 1f : -1f;
        cuerpo.linearVelocity = new Vector2(haciaElCentro * pantallaAnterior.x * 1.18f,
            Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * pantallaAnterior.y * 1.08f));
    }

    void FixedUpdate()
    {
        Vector2 tamano = ActualizarCajas();
        if (tamano == pantallaAnterior)
            return;

        cuerpo.position = new Vector2(
            Mathf.Clamp(cuerpo.position.x, Radio, tamano.x - Radio),
            Mathf.Clamp(cuerpo.position.y, Radio, tamano.y - Radio));
        pantallaAnterior = tamano;
    }

    void LateUpdate()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,
            (Vector2)cuerpo.transform.position * PixelesPorUnidad, null, out Vector2 centro);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas,
            ((Vector2)cuerpo.transform.position + Vector2.one * Radio) * PixelesPorUnidad,
            null, out Vector2 borde);
        imagen.rectTransform.localPosition = centro;
        imagen.rectTransform.sizeDelta = (borde - centro) * 2f;
    }

    Vector2 ActualizarCajas()
    {
        Vector2 tamano = new Vector2(Screen.width, Screen.height) / PixelesPorUnidad;
        ConfigurarCaja(limites[0], new Vector2(tamano.x * 0.5f, -0.5f), new Vector2(tamano.x + 2f, 1f));
        ConfigurarCaja(limites[1], new Vector2(tamano.x * 0.5f, tamano.y + 0.5f), new Vector2(tamano.x + 2f, 1f));
        ConfigurarCaja(limites[2], new Vector2(-0.5f, tamano.y * 0.5f), new Vector2(1f, tamano.y));
        ConfigurarCaja(limites[3], new Vector2(tamano.x + 0.5f, tamano.y * 0.5f), new Vector2(1f, tamano.y));

        for (int i = 0; i < botones.Length; i++)
        {
            botones[i].GetWorldCorners(esquinas);
            Vector2 minimo = RectTransformUtility.WorldToScreenPoint(null, esquinas[0]) / PixelesPorUnidad;
            Vector2 maximo = RectTransformUtility.WorldToScreenPoint(null, esquinas[2]) / PixelesPorUnidad;
            obstaculos[i].enabled = botones[i].gameObject.activeInHierarchy;
            ConfigurarCaja(obstaculos[i], (minimo + maximo) * 0.5f, maximo - minimo);
        }
        for (int i = 0; i < textos.Length; i++)
        {
            var texto = textos[i];
            Bounds tinta = texto.textBounds;
            var caja = cajasTexto[i];
            caja.enabled = texto.isActiveAndEnabled && tinta.size.x > 0f && tinta.size.y > 0f;
            if (!caja.enabled)
                continue;

            Vector2 minimo = PuntoFisico(texto.rectTransform, tinta.min);
            Vector2 maximo = PuntoFisico(texto.rectTransform, tinta.max);
            ConfigurarCaja(caja, (minimo + maximo) * 0.5f, maximo - minimo);
        }

        for (int i = 0; i < iconos.Length; i++)
            ActualizarIcono(i);

        return tamano;
    }

    void ActualizarIcono(int indice)
    {
        Image icono = iconos[indice];
        PolygonCollider2D silueta = siluetas[indice];
        Sprite grafico = icono.sprite;
        silueta.enabled = icono.isActiveAndEnabled && grafico != null;
        if (!silueta.enabled)
            return;

        Rect dibujo = icono.GetPixelAdjustedRect();
        Vector2 minimo = PuntoFisico(icono.rectTransform, dibujo.min);
        Vector2 maximo = PuntoFisico(icono.rectTransform, dibujo.max);
        Rect rectangulo = new Rect(minimo, maximo - minimo);
        if (rectangulo == rectangulosIconos[indice])
            return;

        rectangulosIconos[indice] = rectangulo;
        silueta.transform.position = minimo;
        Vector2 escala = new Vector2(rectangulo.width / grafico.rect.width, rectangulo.height / grafico.rect.height);
        int caminos = grafico.GetPhysicsShapeCount();
        silueta.pathCount = caminos;
        var puntos = new List<Vector2>();
        for (int camino = 0; camino < caminos; camino++)
        {
            grafico.GetPhysicsShape(camino, puntos);
            for (int i = 0; i < puntos.Count; i++)
                puntos[i] = Vector2.Scale(puntos[i] * grafico.pixelsPerUnit + grafico.pivot, escala);
            silueta.SetPath(camino, puntos);
        }
    }

    static Vector2 PuntoFisico(RectTransform rectangulo, Vector2 punto)
    {
        return RectTransformUtility.WorldToScreenPoint(null, rectangulo.TransformPoint(punto)) / PixelesPorUnidad;
    }

    BoxCollider2D CrearCaja(string nombre)
    {
        var caja = new GameObject(nombre);
        caja.transform.SetParent(escenarioFisico.transform, false);
        var collider = caja.AddComponent<BoxCollider2D>();
        collider.sharedMaterial = material;
        return collider;
    }

    void ConfigurarCaja(BoxCollider2D caja, Vector2 centro, Vector2 tamano)
    {
        if ((Vector2)caja.transform.position != centro)
            caja.transform.position = centro;
        if (caja.size != tamano)
            caja.size = tamano;
    }

    Sprite CrearSprite()
    {
        const int lado = 96;
        textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Clamp;
        var pixeles = new Color[lado * lado];
        for (int y = 0; y < lado; y++)
        for (int x = 0; x < lado; x++)
        {
            float distancia = new Vector2(x + 0.5f - lado / 2f, y + 0.5f - lado / 2f).magnitude;
            pixeles[y * lado + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(lado / 2f - distancia));
        }
        textura.SetPixels(pixeles);
        textura.Apply();
        sprite = Sprite.Create(textura, new Rect(0, 0, lado, lado), Vector2.one * 0.5f,
            lado, 0, SpriteMeshType.FullRect);
        return sprite;
    }

    void OnDestroy()
    {
        Destroy(escenarioFisico);
        Destroy(sprite);
        Destroy(textura);
        Destroy(material);
    }
}

