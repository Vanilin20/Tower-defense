# 📋 Інструкція налаштування бойової системи

## 🎯 **Що потрібно зробити:**

### 1. **Створити GameObject для CombatSystemManager**

1. Відкрийте сцену `Test.unity` або `Menu.unity`
2. Створіть новий GameObject: **ПКМ → Create Empty**
3. Назвіть його `CombatSystemManager`
4. Додайте компонент `CombatSystemManager` (Scripts → CombatSystemManager)

### 2. **Перевірити наявність HeightManager**

Якщо HeightManager ще не створений:
1. Створіть новий GameObject: **ПКМ → Create Empty**
2. Назвіть його `HeightManager`
3. Додайте компонент `HeightManager` (Scripts → HeightManager)
4. Налаштуйте доступні висоти в інспекторі

### 3. **Налаштування в інспекторі**

#### **CombatSystemManager:**
- ✅ `Enable Debug Logs` - увімкнути/вимкнути debug логи
- ✅ `Enable Combat Utils` - увімкнути/вимкнути утиліти бою
- ✅ `Enable Death Handler` - увімкнути/вимкнути обробку смерті

#### **HeightManager:**
- ✅ `Available Heights` - список доступних висот (0, 2, 4, 6, 8)
- ✅ `Height Tolerance` - толерантність для визначення висоти (0.1)
- ✅ `Max Units Per Height` - максимум юнітів на висоті (10)

## 🔧 **Автоматичне створення:**

Якщо HeightManager не знайдено, CombatSystemManager автоматично створить його з базовими налаштуваннями.

## 🧪 **Тестування системи:**

1. Запустіть гру
2. В консолі повинні з'явитися повідомлення:
   ```
   🔧 Ініціалізація бойової системи...
   ✅ Бойова система ініціалізована!
   ```

3. Для тестування введіть в консоль:
   ```csharp
   CombatSystemManager.Instance.TestCombatSystem();
   ```

## 📁 **Структура файлів:**

```
Scripts/
├── CombatSystemManager.cs    ← НОВИЙ (потрібно прив'язати до GameObject)
├── CombatUtils.cs           ← Статичний клас (не потребує прив'язки)
├── DeathHandler.cs          ← Статичний клас (не потребує прив'язки)
├── HeightManager.cs         ← Існуючий (потрібно прив'язати до GameObject)
└── Unit.cs                  ← Оптимізований
```

## ⚠️ **Важливо:**

- **CombatSystemManager** та **HeightManager** повинні бути на сцені
- **CombatUtils** та **DeathHandler** - статичні класи, не потребують GameObject
- Всі юніти автоматично використовують нові утиліти
- Система працює з існуючим кодом без змін

## 🎮 **Перевірка роботи:**

1. Створіть героя та ворога на сцені
2. Запустіть гру
3. Перевірте консоль на наявність повідомлень про атаки
4. Всі атаки тепер використовують оптимізований код 