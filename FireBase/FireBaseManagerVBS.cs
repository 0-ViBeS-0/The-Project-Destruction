using Firebase.Auth;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System;

public class FireBaseManagerVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# SINGLETON")]
    public static FireBaseManagerVBS instance;

    [Header("|-# LOGIN")]
    [SerializeField] private TMP_InputField _emailInputLOG;
    [SerializeField] private TMP_InputField _passwordInputLOG;
    [SerializeField] private TMP_InputField _emailInputREG;
    [SerializeField] private TMP_InputField _passwordInputREG;

    [Header("|-# PROFILE")]
    [SerializeField] private TMP_Text _id;
    [SerializeField] private TMP_InputField _name;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private FirebaseFirestore db;

    #endregion

    #region Awake/Start/OnApplicationQuit

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        CheckUser();
    }

    private void OnApplicationQuit()
    {
        if (currentUser != null)
            SetStatus(false);
    }

    #endregion

    #region Авто-проверка входа

    private void CheckUser()
    {
        currentUser = auth.CurrentUser;

        if (currentUser != null)
        {
            Debug.Log("Автоматический вход в: " + currentUser.Email); //
            LoadProfileData(currentUser.UserId);
            LauncherVBS.instance.ConnectToServer(true);
        }
        else
        {
            Debug.Log("Войдите в аккаунт!"); //
            OpenMenuAndPanel("login", "");
        }
    }

    #endregion

    #region Регистрация/Вход/Выход

    public void LoginOnGuest()
    {
        LauncherVBS.instance.ConnectToServer(false);
        GameData.MyPlayerID = "GUEST";
        _id.text = GameData.MyPlayerID;
        GameData.MyName = "DAUN " + UnityEngine.Random.Range(0000, 9999);
        _name.text = GameData.MyName;
        GameData.MyUserID = "WWW.guest.com";
        LauncherVBS.instance.SetNickName(GameData.MyName);
    }

    public void RegisterUser()
    {
        if (string.IsNullOrEmpty(_emailInputREG.text) && string.IsNullOrEmpty(_passwordInputREG.text))
        {
            Debug.Log("Какой-то из полей, ПУСТ!"); //
            return;
        }

        string email = _emailInputREG.text;
        string password = _passwordInputREG.text;

        ///////////////////////////////////////////////////////////////////////////////////////////////// ПРОВЕРКА ПАРОЛЯ
        if (password.Length < 6)
        {
            Debug.Log("Пароль должен содержать больше 6 символов!"); //
            return;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => // Создание
        {
            if (task.IsCanceled)
            {
                Debug.Log("Регистрация ОТМЕНЕНА!"); //
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("Ошибка регистрации: " + task.Exception); //
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("Регистрация УСПЕШНА! Почта: " + newUser.Email); //
            OpenMenuAndPanel("loading", "");

            string userId = newUser.UserId;
            SaveProfileData(userId);

            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => // Вход после успешной регистрации
            {
                if (task.IsCanceled)
                {
                    Debug.Log("Вход ОТМЕНЕН!"); //
                    OpenMenuAndPanel("login", "");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("Ошибка ВХОДА: " + task.Exception); //
                    OpenMenuAndPanel("login", "");
                    return;
                }

                Debug.Log("Вход УСПЕШЕН! Почта: " + newUser.Email); //
                LoadProfileData(newUser.UserId);
                LauncherVBS.instance.ConnectToServer(true);
                currentUser = auth.CurrentUser;
                GenerateID(userId);
            });
        });
    }

    public void LoginUser()
    {
        if (string.IsNullOrEmpty(_emailInputLOG.text) && string.IsNullOrEmpty(_passwordInputLOG.text))
        {
            Debug.Log("Какой-то из полей, ПУСТ!"); //
            return;
        }

        string email = _emailInputLOG.text;
        string password = _passwordInputLOG.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("Вход ОТМЕНЕН!"); //
                return;
            }
            if (task.IsFaulted)
            {
                Debug.Log("Ошибка ВХОДА: " + task.Exception); //
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log("Вход УСПЕШЕН! Почта: " + user.Email); //
            LoadProfileData(user.UserId);
            LauncherVBS.instance.ConnectToServer(true);
            currentUser = auth.CurrentUser;
        });
    }

    public void LogoutUser()
    {
        auth.SignOut();
        Debug.Log("Вы вышли с аккаунта!"); //
        OpenMenuAndPanel("loading", "");
        LauncherVBS.instance.DisconnectFromServer();
    }

    #endregion

    #region FireBase Данные профиля

    private void SaveProfileData(string userId) // Сохранение данные НОВОГО пользователя
    {
        DocumentReference docRef = db.Collection("users").Document(userId); // Путь
        Dictionary<string, object> user = new() // Словарь
        {
            { "createdAt", DateTime.Now.ToString() },
            { "userID", "NULL" },
            { "username", GenerateName() },
            { "status", "offline" }
        };

        docRef.SetAsync(user).ContinueWithOnMainThread(task => // Сохранение
        {
            ///////////////////////////////////////////////////////////////////////////////////////////////// ОБНАРУЖЕНИЕ ОШИБОК
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Ошибка сохранения профиля: " + task.Exception); //
                return;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////
            Debug.Log("Профиль СОХРАНЕН!"); //
        });
    }

    private void LoadProfileData(string userId) // Загрузка данных пользователя
    {
        DocumentReference docRef = db.Collection("users").Document(userId); // Путь
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => // Загрузка
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> userData = task.Result.ToDictionary();
                string username = userData["username"].ToString();
                string userID = userData["userID"].ToString();

                Debug.Log("Загрузка УСПЕШНА! " + username + " ( | ID: " + userID + ")"); //
                LauncherVBS.instance.SetNickName(username);
                _id.text = userID;
                _name.text = username;

                GameData.MyUserID = userId;
                GameData.MyPlayerID = userID;
                GameData.MyName = username;
            }
            else
            {
                Debug.Log("Данные не найдены!(");
            }
        });
    }

    #endregion

    #region FireBase Данные инвентаря скинов



    #endregion

    #region Другое

    private void GenerateID(string userId)
    {
        DocumentReference docRef = db.Collection("Global").Document("Stat"); // Путь
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => // Загрузка
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    long currentValue = snapshot.GetValue<long>("Players");
                    long newValue = currentValue + 1;

                    Dictionary<string, object> updates = new() // Словарь
                    {
                        { "Players", newValue }
                    };

                    docRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask => // Обновление (счетчик)
                    {
                        if (updateTask.IsCompleted)
                        {
                            Debug.Log("Счетчик успешно обновлен до: " + newValue);

                            DocumentReference docRef = db.Collection("users").Document(userId); // Путь
                            Dictionary<string, object> updateID = new() // Словарь
                            {
                                { "userID", newValue.ToString("D10") }
                            };

                            docRef.UpdateAsync(updateID).ContinueWithOnMainThread(task => // Обновление (профиль)
                            {
                                if (task.IsCanceled || task.IsFaulted)
                                {
                                    Debug.Log("Ошибка изменения ID: " + task.Exception);
                                }
                                else
                                {
                                    Debug.Log("ID создан и изменен!");
                                    _id.text = newValue.ToString("D10");
                                }
                            });
                        }
                        else
                        {
                            Debug.LogError("Ошибка при обновлении счетчика: " + updateTask.Exception);
                            return;
                        }
                    });
                }
                else
                {
                    Debug.Log("Документ не существует.");
                    return;
                }
            }
            else
            {
                Debug.Log("globalStat не найден!");
                return;
            }
        });
    }

    private string GenerateName()
    {
        return "Player " + UnityEngine.Random.Range(0000, 9999);
    }

    public void UpdateUsername(string newUsername) // Изменить имя
    {
        string userId = currentUser.UserId;
        DocumentReference docRef = db.Collection("users").Document(userId); // Путь

        Dictionary<string, object> updates = new() // Словарь
        {
            { "username", newUsername }
        };

        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task => // Обновление имени
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Ошибка изменения имени: " + task.Exception);
            }
            else
            {
                Debug.Log("Имя изменено!");

                LauncherVBS.instance.SetNickName(newUsername);
                _name.text = newUsername;
            }
        });
    }

    private void OpenMenuAndPanel(string menu, string panel)
    {
        MenuManager.instance.OpenMenu(menu);
        MenuManager.instance.OpenPanel(panel);
    }

    public void SetStatus(bool status)
    {
        string userId = currentUser.UserId;
        DocumentReference docRef = db.Collection("users").Document(userId); // Путь

        Dictionary<string, object> updates = new() // Словарь
        {
            { "status", status ? "online" : "offline" }
        };

        docRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Ошибка изменения статуса: " + task.Exception);
            }
            else
            {
                Debug.Log("Статус изменен: " + (status ? "online" : "offline"));
            }
        });
    }

    #endregion
}
