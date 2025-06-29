# Руководство по страницам "Мои заказы" и "Профиль"

## Обзор

Были созданы две новые страницы для пользователей ювелирного интернет-магазина:

1. **Страница "Мои заказы"** (`/orders`) - для просмотра истории заказов пользователя
2. **Страница "Профиль"** (`/profile`) - для просмотра и редактирования профиля пользователя

## Функциональность

### Страница "Мои заказы" (/orders)

**Возможности:**
- Просмотр всех заказов пользователя
- Отображение статуса каждого заказа с цветовой индикацией
- Просмотр деталей заказа (товары, количество, цены)
- Отображение адреса доставки и примечаний
- Возможность отмены заказов со статусом "Ожидает обработки"

**Статусы заказов:**
- 🟡 **Ожидает обработки** (Pending) - заказ принят, но еще не обработан
- 🔵 **В обработке** (Processing) - заказ обрабатывается
- 🟣 **Отправлен** (Shipped) - заказ отправлен покупателю
- 🟢 **Доставлен** (Delivered) - заказ успешно доставлен
- 🔴 **Отменен** (Cancelled) - заказ отменен

**Интерфейс:**
- Карточки заказов с основной информацией
- Кнопка "Подробнее" для просмотра деталей
- Кнопка "Отменить" для заказов в статусе "Ожидает обработки"
- Автоматическое обновление списка

### Страница "Профиль" (/profile)

**Возможности:**
- Просмотр информации о профиле пользователя
- Редактирование личных данных
- Просмотр статистики (заказы, покупки)
- Управление безопасностью (планируется)

**Вкладки:**

#### 1. Информация о профиле
- Основная информация (имя пользователя, email, имя, фамилия)
- Контактные данные (телефон, адрес)
- Дата регистрации
- Статистика заказов и покупок

#### 2. Редактировать профиль
- Форма редактирования личных данных
- Валидация полей
- Сохранение изменений
- Отмена редактирования

#### 3. Безопасность
- Смена пароля (планируется)
- Настройки уведомлений (планируется)

## Технические детали

### API Endpoints

#### Профиль пользователя:
- `GET /api/auth/profile` - получение профиля текущего пользователя
- `PUT /api/auth/profile` - обновление профиля пользователя

#### Заказы:
- `GET /api/orders/user` - получение заказов текущего пользователя
- `GET /api/orders/{id}` - получение деталей конкретного заказа

### Компоненты

#### Blazor страницы:
- `Pages/Orders.razor` - страница заказов
- `Pages/Profile.razor` - страница профиля

#### Сервисы:
- `Services/IUserService.cs` - интерфейс сервиса пользователей
- `Services/UserService.cs` - реализация сервиса пользователей
- `Services/IOrderService.cs` - интерфейс сервиса заказов (обновлен)

#### Модели:
- `Models/UserViewModel.cs` - модели пользователя
- `Models/UpdateProfileModel.cs` - модель для обновления профиля

### Навигация

Ссылки на новые страницы добавлены в главное меню (`Shared/NavMenu.razor`):
- "Мои заказы" - отображается только для авторизованных пользователей
- "Профиль" - отображается только для авторизованных пользователей

## Запуск и тестирование

### 1. Запуск приложения

```bash
# Запуск API
cd src/JewelryStore.API
dotnet run

# Запуск Blazor UI (в новом терминале)
cd src/JewelryStore.BlazorUI
dotnet run
```

### 2. Тестирование

1. Откройте браузер и перейдите на `http://localhost:5216`
2. Зарегистрируйтесь или войдите в систему
3. В меню появятся ссылки "Мои заказы" и "Профиль"
4. Протестируйте функциональность:
   - Просмотр и редактирование профиля
   - Создание заказа и просмотр в разделе "Мои заказы"

### 3. Автоматическое тестирование

Запустите тестовый скрипт:

```powershell
powershell -ExecutionPolicy Bypass -File test-profile-pages.ps1
```

## Безопасность

- Все endpoints защищены JWT аутентификацией
- Пользователи могут видеть только свои заказы и профиль
- Валидация данных на клиенте и сервере
- Проверка прав доступа на уровне API

## Планы развития

### Ближайшие улучшения:
1. **Смена пароля** - добавление функции изменения пароля
2. **Настройки уведомлений** - управление email/SMS уведомлениями
3. **Детальная страница заказа** - отдельная страница с полной информацией о заказе
4. **Отмена заказов** - реализация функции отмены заказов через API
5. **Фильтрация заказов** - поиск и фильтрация по статусу, дате, сумме

### Дополнительные возможности:
1. **История изменений профиля** - логирование изменений
2. **Избранные товары** - список желаний пользователя
3. **Адресная книга** - сохранение нескольких адресов доставки
4. **Программа лояльности** - бонусы и скидки
5. **Отзывы и рейтинги** - возможность оставлять отзывы о товарах

## Поддержка

При возникновении проблем:
1. Проверьте, что API и Blazor приложение запущены
2. Убедитесь, что пользователь авторизован
3. Проверьте консоль браузера на наличие ошибок
4. Проверьте логи API сервера

## Структура файлов

```
JewelryStore/
├── src/
│   ├── JewelryStore.API/
│   │   ├── Controllers/
│   │   │   └── AuthController.cs (обновлен)
│   │   └── ...
│   ├── JewelryStore.BlazorUI/
│   │   ├── Pages/
│   │   │   ├── Orders.razor (новый)
│   │   │   └── Profile.razor (новый)
│   │   ├── Services/
│   │   │   ├── IUserService.cs (новый)
│   │   │   └── UserService.cs (новый)
│   │   ├── Models/
│   │   │   └── UserViewModel.cs (обновлен)
│   │   ├── Shared/
│   │   │   └── NavMenu.razor (обновлен)
│   │   └── Program.cs (обновлен)
│   └── JewelryStore.Core/
│       └── DTOs/
│           └── AuthDtos.cs (обновлен)
├── test-profile-pages.ps1 (новый)
└── PROFILE_PAGES_GUIDE.md (этот файл)
``` 