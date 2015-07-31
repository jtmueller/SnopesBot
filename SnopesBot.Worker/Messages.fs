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