# TestHorror
Пятиминутная демка а-ля FearsToFathom/подобные игры хоррор.\
Главный упор при разработке был сделан в архитектуру, демонстрацию кода и его реальной работе.

<img width="3440" height="1440" alt="Testhorror Screenshot 2026 03 15 - 19 23 42 12" src="https://github.com/user-attachments/assets/96aaa27e-d69c-4b1b-b77a-b66358ec8e41" />

<img width="2694" height="1122" alt="Screenshot 2026-03-15 192838" src="https://github.com/user-attachments/assets/052c2901-b577-4431-913d-d4f20188a456" />

## Как играть
`WASD` - Пермещение\
`E` - Взаимодействие/Пропуск фраз в диалогах

<img width="1953" height="813" alt="Screenshot 2026-03-15 193754" src="https://github.com/user-attachments/assets/e40f38d4-c878-47ee-ba86-4d7204ef8d8e" />

## Основная информация
- Версия движка - Unity 6000.2.8f1 - 6000.3.11f1
- Графический пайплайн - URP

Большая часть кода и ключевая архитектура написаны мной\
Для оптимизации времени, использовал LLM для написания реализаций минорных функций

Часть анимаций нпс и все анимации игрока сделаны мной, остальные скачаны с миксамо

![ezgif-53cf9316c84a917b](https://github.com/user-attachments/assets/b6f419e5-a123-4e68-8b58-ba9c84c42bd3)

## Основные цели архитектуры
- Масштабируемая система взаимодействий игрока, нпс и мира 
- Масштабируемая, простая и открытая система режиссуры геймплея через диалоговые реплики и анимации 
- Реализация возможности писать простые, читаемые "сценарии" сцен через ассинхронные UniTask'и

<img width="2689" height="1121" alt="Screenshot 2026-03-15 193418" src="https://github.com/user-attachments/assets/03255315-27e8-45bb-9c65-6a01170871e1" />

## Использованные ассеты
- [UniTask](https://github.com/Cysharp/UniTask)
- [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676?srsltid=AfmBOoqu12vdD7pwySoDknoSnadHAZN67AZKh8S83W4-vUJPrvaaJ3Cl)
- [Набор кухонных ассетов](https://assetstore.unity.com/packages/3d/props/food/pandazole-kitchen-food-low-poly-pack-204525)
- [Локация кафе](https://assetstore.unity.com/packages/3d/environments/coffee-shop-environment-217600)
- [Набор аналогово-хоррорных звуковых эффектов](https://assetstore.unity.com/packages/audio/music/synth-pack-01-159857)
- Анимации с [Mixamo.com](https://www.mixamo.com/)
- Звуки с [Pixabay.com](https://pixabay.com/)
- https://sketchfab.com/3d-models/mechanic-low-poly-character-81b1d3d665f04345a4c47e5418b8e6ad
- https://sketchfab.com/3d-models/casual-male-architectural-updated-f4e1f9f3463141188281b2212d62a76d
- Шрифты с авторами и лицензиями лежат в проекте
