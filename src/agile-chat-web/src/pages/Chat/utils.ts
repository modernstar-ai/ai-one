import { ChatType, Message, MessageType } from '@/types/ChatThread';

export enum ResponseType {
  Chat = 'Chat',
  DbMessages = 'DbMessages',
}

export enum TempIdType {
  User = '-1',
  Assistant = '-2',
}

export async function* consumeChunks(
  reader: ReadableStreamDefaultReader<Uint8Array | undefined>,
  decoder: TextDecoder
) {
  let partialChunk = '';
  while (true) {
    const { done, value } = await reader.read();

    partialChunk += decoder.decode(value, { stream: true });

    let belIndex;
    while ((belIndex = partialChunk.indexOf('')) > -1) {
      const completeChunk = partialChunk.slice(0, belIndex);
      partialChunk = partialChunk.slice(belIndex + 1);
      if (completeChunk) {
        try {
          const obj = JSON.parse(completeChunk);
          yield obj;
        } catch (e) {
          console.log('Chunk parsing error: ', e);
        }
      }
    }

    if (done) {
      break;
    }
  }
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
    options: { isDisliked: false, isLiked: false, metadata: {} },
  };
};
