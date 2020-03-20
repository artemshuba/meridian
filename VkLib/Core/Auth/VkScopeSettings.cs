using System;

namespace VkLib.Auth
{
    /// <summary>
    /// Права доступа приложения
    /// </summary>
    [Flags]
    public enum VkScopeSettings
    {
        /// <summary>
        /// Пользователь разрешил отправлять ему уведомления
        /// </summary>
        CanNotify = 1,
        /// <summary>
        /// Доступ к друзьям
        /// </summary>
        CanAccessFriends = 2,
        /// <summary>
        /// Доступ к фотографиям
        /// </summary>
        CanAccessPhotos = 4,
        /// <summary>
        /// Доступ к аудиозаписям
        /// </summary>
        CanAccessAudios = 8,
        /// <summary>
        /// Доступ к видеозаписям
        /// </summary>
        CanAccessVideos = 16,
        /// <summary>
        /// Доступ к предложениям
        /// </summary>
        [Obsolete]
        CanAccessOffers = 32,
        /// <summary>
        /// Доступ к вопросам
        /// </summary>
        [Obsolete]
        CanAccessQuestions = 64,
        /// <summary>
        /// Доступ к wiki-страницам
        /// </summary>
        CanAccessWiki = 128,
        /// <summary>
        /// Добавление ссылки на приложение в меню слева
        /// </summary>
        CanAddAppLinkToUserMenu = 256,
        /// <summary>
        /// Доступ к статусам пользователя
        /// </summary>
        CanAccessStatus = 1024,
        /// <summary>
        /// Доступ заметкам пользователя
        /// </summary>
        CanAccessNotes = 2048,
        /// <summary>
        /// Доступ к расширенным методам работы с сообщениями
        /// </summary>
        CanAccessMessages = 4096,
        /// <summary>
        /// Доступ к обычным и расширенным методам работы со стеной
        /// </summary>
        CanAccessWall = 8192,
        /// <summary>
        /// Доступ к функциям для работы с рекламным кабинетом
        /// </summary>
        CanAccessAdsCabinet = 32768,
        /// <summary>
        /// Доступ к документам пользователя
        /// </summary>
        CanAccessDocs = 131072,
        /// <summary>
        /// Доступ к группам пользователя
        /// </summary>
        CanAccessGroups = 262144,
        /// <summary>
        /// God mode (offline доступ)
        /// </summary>
        IamTheGod = 999999 //вечный токен
        //там еще всякая фигня есть, которая тут не нужна
    }
}
