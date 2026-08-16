# Tamagotchi Estable

Prototipo para game jam desarrollado en Unity 6.  
Tema: **Múltiples soluciones** — el jugador puede ganar con estrategias distintas, todas válidas.

---

## Concepto

Durante 30 días, elegís actividades para tu personaje intentando mantener su ánimo dentro de una **zona segura (30–75)**. Ni muy triste ni muy feliz. No hay una única estrategia correcta: podés jugar seguro y lento, o arriesgado y rápido.

---

## Cómo jugar

- Cada día tenés energía limitada (40–100 puntos al despertar).
- Elegís actividades del menú: **Actividades**, **Parque**, **Social** o **Descansar**.
- Cada actividad consume energía y modifica el ánimo.
- Cuando la energía llega a 0 (o descansás), termina el día.
- Si terminás 3 días seguidos con el ánimo fuera de la zona segura → **Game Over**.
- Si llegás al día 30 → **Victoria**.

---

## Arquetipos de juego

| Arquetipo | Estrategia |
|---|---|
| **Estabilizador** | Solo actividades de bajo impacto (Leer, Pasear), varias veces por día. Corrección lenta pero segura. |
| **Especialista** | Solo actividades de alto impacto (Ejercicio, Visitar Amigos). Corrección rápida pero arriesga pasarse de largo. |
| **Mixto** | Alterna según el estado del día. Más eficiente, requiere más atención. |

Los tres llegan al día 30 si jugás bien. Ninguno es la respuesta "correcta".

---

## Actividades

| Actividad | Impacto | Energía | Δ Ánimo |
|---|---|---|---|
| Leer | Bajo | 15 | +6 |
| Pasear | Bajo | 13 | +5 |
| Ejercicio Intenso | Alto | 38 | +18 |
| Visitar Amigos | Alto | 32 | +20 |
| Descansar | Especial | +20 recupera | termina el día |

> Cuando una actividad se usa mucho, el personaje se fatiga: el costo de energía sube x1.5 y la recompensa de ánimo baja a x0.4. La fatiga se recupera descansando la actividad un día.

---

## Arquitectura

```
Assets/
├── Scripts/
│   ├── ActivityData.cs          # ScriptableObject con datos de cada actividad
│   ├── GameManager.cs           # Singleton: lógica central, estados, eventos
│   ├── UIBuilder.cs             # Construye toda la UI por código
│   ├── ActivityButton.cs        # Botón de actividad con estado de fatiga
│   ├── CharacterAnimator.cs     # Animaciones del personaje
│   ├── DayTransitionController.cs
│   ├── FatigueDisplay.cs
│   ├── DebugPanel.cs            # Panel de debug (solo Editor/Dev builds)
│   └── Editor/
│       └── ActivityDataGenerator.cs  # Menú: Tamagotchi > Generate Default Activities
├── ScriptableObjects/
│   └── Activities/              # 5 assets de actividades
└── Resources/
    └── Icons/                   # Íconos de los botones del menú
```

---

## Setup

1. Abrir el proyecto en Unity 6
2. Menú **Tamagotchi > Generate Default Activities**
3. Crear GameObject `GameManager` → Add Component `GameManager` → asignar los 5 activity assets
4. Crear GameObject `UI` → Add Component `Canvas` + `UIBuilder`
5. `Ctrl+S` → Play

---

## Balance (valores por defecto, ajustables en GameManager)

| Variable | Valor |
|---|---|
| Ánimo inicial | 50 |
| Zona segura | 30 – 75 |
| Fluctuación diaria | ±8 |
| Energía al despertar | 40 – 100 |
| Días inestables para game over | 3 |

---

## Paleta visual

| Rol | Color |
|---|---|
| Fondo | `#0D0F0A` |
| Panel | `#1A1F14` |
| Verde neón | `#9BF026` |
| Verde oscuro | `#4A7C00` |

---

## Stack

- Unity 6 (URP)
- C# — sin dependencias externas
- UI construida 100% por código (no prefabs)

---

## Créditos y recursos

### Íconos
Íconos pixel art provistos por [Flaticon](https://www.flaticon.com/free-icons/pixel-art) — uso gratuito con atribución.

| Ícono | Uso en el juego |
|---|---|
| arcade-machine | Botón ACTIVIDADES |
| tree | Botón PARQUE |
| feedback-emoji | Botón SOCIAL |
| youtube | Botón DESCANSAR |
