namespace SnopesBot.Worker

open System

// Telegraph message types

type User = {
    id: int
    first_name: string
    last_name: string
    username: string
}

type GroupChat = {
    id: int
    title: string
}

type PhotoSize = {
    file_id: string
    width: int
    height: int
    file_size: int option
}

type Audio = {
    file_id: string
    duration: int
    mime_type: string option
    file_size: int option
}

type Document = {
    file_id: string
    thumb: PhotoSize option
    file_name: string option
    mime_type: string option
    file_size: int option
}

type Sticker = {
    file_id: string
    width: int
    height: int
    thumb: PhotoSize option
    file_size: int option
}

type Video = {
    file_id: string
    width: int
    height: int
    duration: int
    thumb: PhotoSize option
    mime_type: string option
    file_size: string option
}

type Contact = {
    phone_number: string
    first_name: string
    last_name: string option
    user_id: int option
}

type Location = {
    longitude: float
    latitude: float
}

type UserProfilePhotos = {
    total_count: int
    photos: PhotoSize[][]
}

type ReplyKeyboardMarkup = {
    keyboard: string[][]
    resize_keyboard: bool option
    one_time_keyboard: bool option
    selective: bool option
}

type ReplyKeyboardHide = {
    hide_keyboard: bool
    selective: bool option
}

type ForceReply = {
    force_reply: bool
    selective: bool option
}

type ChatType =
    | User of User
    | GroupChat of GroupChat

    member x.chat_id =
        match x with
        | User u -> u.id
        | GroupChat g -> g.id

type MessageBody =
    | Text of string
    | Audio of Audio
    | Document of Document
    | Photo of PhotoSize[]
    | Sticker of Sticker
    | Video of Video
    | Contact of Contact
    | Location of Location
    | UserJoined of User
    | UserLeft of User
    | NewChatTitle of string
    | NewChatPhoto of PhotoSize[]
    | DeleteChatPhoto
    | GroupChatCreated
    | NoMessage

type Message = {
    message_id: int
    from: User
    date: DateTimeOffset
    chat: ChatType
    body: MessageBody
    forward_from: User option
    forward_date: DateTimeOffset option
    reply_to_message: Message option
    caption: string option
}

type ReplyMarkup =
    | KeyboardMarkup of ReplyKeyboardMarkup
    | KeyboardHide of ReplyKeyboardHide
    | ForceReply of ForceReply

type SendMessage = {
    chat_id: int
    text: string
    disable_web_page_preview: bool option
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendPhoto = {
    chat_id: int
    photo: string
    caption: string option
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendAudio = {
    chat_id: int
    audio: string
    duration: int option
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendDocument = {
    chat_id: int
    document: string
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendSticker = {
    chat_id: int
    sticker: string
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendVideo = {
    chat_id: int
    video: string
    duration: int option
    caption: string option
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type SendLocation = {
    chat_id: int
    latitude: float
    longitude: float
    reply_to_message_id: int option
    reply_markup: ReplyMarkup option
}

type ChatAction =
    | typing = 1
    | upload_photo = 2
    | record_video = 3
    | upload_video = 4
    | record_audio = 5
    | upload_audio = 6
    | upload_document = 7
    | find_location = 8

type TelegramBotMethod =
    | GetMe
    | SendMessage of SendMessage
    | ForwardMessage of chat_id:int * from_chat_id:int * message_id:int
    | SendPhoto of SendPhoto
    | SendAudio of SendAudio
    | SendDocument of SendDocument
    | SendLocation of SendLocation
    | SendChatAction of chat_id:int * action:ChatAction
    | GetUserProfilePhotos of user_id:int

