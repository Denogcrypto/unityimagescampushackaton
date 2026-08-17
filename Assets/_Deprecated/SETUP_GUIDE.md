# Tamagotchi Estable — Guía de Setup en Unity

## Paso 0: Generar los ActivityData
1. Menú Unity: **Tamagotchi > Generate Default Activities**
2. Se crean 5 assets en `Assets/ScriptableObjects/Activities/`

---

## Paso 1: GameManager (mínimo viable con debug)

1. Crea un GameObject vacío en la escena, llámalo `GameManager`.
2. Añade el componente `GameManager`.
3. En el Inspector, arrastra los 5 ActivityData al array **All Activities**.
4. Crea otro GameObject vacío `DebugPanel` y añade el componente `DebugPanel`.
5. **Play** → usa los botones OnGUI para validar victoria/derrota sin UI real.

---

## Paso 2: CharacterAnimator

1. Crea un GameObject con `SpriteRenderer` para el personaje.
2. Añade `CharacterAnimator`.
3. Asigna el `SpriteRenderer` en el Inspector.
4. (Opcional) Crea un Animator Controller con un trigger `PlayActivity` y asígnalo.

---

## Paso 3: Canvas / UI

### Jerarquía sugerida
```
Canvas
├── HUD
│   ├── MoodBar (Slider)         ← min=0, max=1, interactable=false
│   │   ├── SafeZoneMarkerLeft   (Image)
│   │   └── SafeZoneMarkerRight  (Image)
│   ├── EnergyBar (Slider)
│   ├── DayLabel (TextMeshPro)
│   └── UnstableWarning (GameObject)
│       └── StreakLabel (TextMeshPro)
├── MainMenuPanel
│   ├── Btn_Actividades  → ShowActivitiesPanel()
│   ├── Btn_Parque       → ShowParkPanel()
│   ├── Btn_Social       → ShowSocialPanel()
│   └── Btn_Descansar    → ActivityButton (activity = Descansar)
├── ActivitiesPanel
│   ├── Btn_Leer         → ActivityButton (activity = Leer)
│   ├── Btn_EjercicioInt → ActivityButton (activity = Ejercicio Intenso)
│   └── Btn_Volver       → ShowMainMenu()
├── ParkPanel
│   ├── Btn_Pasear       → ActivityButton (activity = Pasear)
│   └── Btn_Volver       → ShowMainMenu()
├── SocialPanel
│   ├── Btn_VisitarAmigos → ActivityButton (activity = Visitar Amigos)
│   └── Btn_Volver        → ShowMainMenu()
├── DayTransitionPanel   → DayTransitionController
├── GameOverPanel
│   ├── GameOverLabel (TextMeshPro)
│   └── Btn_Restart       → UIManager.RestartGame()
└── VictoryPanel
    └── Btn_Restart        → UIManager.RestartGame()
```

### UIManager
1. Crea un GameObject `UIManager` y añade el componente `UIManager`.
2. Arrastra todos los paneles y elementos HUD a sus slots correspondientes.
3. Los botones de navegación usan **On Click()** → UIManager → ShowXxxPanel().

### ActivityButton
- Cada botón de actividad tiene el componente `ActivityButton`.
- Asigna el `ActivityData` correspondiente en el campo **Activity**.
- Asigna los sprites `lowRiskSprite` / `highRiskSprite` (íconos dither monocromo).

---

## Valores de balance (ajustables en GameManager)

| Variable | Default | Descripción |
|---|---|---|
| moodInitial | 50 | Ánimo al iniciar |
| moodSafeMin | 30 | Límite inferior zona segura |
| moodSafeMax | 75 | Límite superior zona segura |
| moodFluctuationMin | -8 | Fluctuación mínima al inicio del día |
| moodFluctuationMax | +8 | Fluctuación máxima al inicio del día |
| energyRecoveryMin | 40 | Energía mínima al despertar |
| energyRecoveryMax | 100 | Energía máxima al despertar |
| maxUnstableDays | 3 | Días inestables consecutivos = game over |

---

## Arquetipos para validar en playtesting

| Arquetipo | Estrategia | ¿Cómo verificar? |
|---|---|---|
| Estabilizador | Solo Leer + Pasear, múltiples veces/día | Llega al día 30, corrección lenta |
| Especialista | Solo Ejercicio + Visitar Amigos | Llega al día 30, riesgo de sobrepaso |
| Mixto | Alterna según el ánimo del día | Más eficiente, requiere más atención |

---

## Paleta visual sugerida (Game Boy Green)
```
Negro fondo:   #0D0D0D
Verde oscuro:  #1A2B00
Verde medio:   #4A7C00
Verde neón:    #9BF026
```
