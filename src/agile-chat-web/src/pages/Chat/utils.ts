import { ChatType, Message, MessageType } from '@/types/ChatThread';

export enum ResponseType {
  Chat = 'Chat',
  DbMessages = 'DbMessages'
}

export enum TempIdType {
  User = '-1',
  Assistant = '-2'
}

export const updateMessages = (messages: Message[], newMessage: Message): Message[] => {
  return messages.map((message) => {
    if (
      (message.id === TempIdType.User && newMessage.messageType === MessageType.User) ||
      (message.id === TempIdType.Assistant && newMessage.messageType === MessageType.Assistant)
    ) {
      return newMessage;
    }

    return message;
  });
};

export const createTempMessage = (content: string, messageType: MessageType): Message => {
  return {
    type: ChatType.Message,
    messageType: messageType,
    content: content,
    id: messageType === MessageType.User ? TempIdType.User : TempIdType.Assistant,
    threadId: '',
    createdDate: new Date(Date.now()),
    lastModified: new Date(Date.now()),
    options: { IsDisliked: false, IsLiked: false }
  };
};
