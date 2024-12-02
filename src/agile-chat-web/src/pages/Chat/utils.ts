import { ChatType, Message } from "@/types/ChatThread";

export enum ResponseType {
  Chat = "Chat",
  Citations = "Citations",
  DbMessages = "DbMessages",
}

export async function* consumeChunks(
  reader: ReadableStreamDefaultReader<Uint8Array | undefined>,
  decoder: TextDecoder
) {
  let partialChunk = "";
  while (true) {
    const { done, value } = await reader.read();

    partialChunk += decoder.decode(value, { stream: true });

    let belIndex;
    while ((belIndex = partialChunk.indexOf("")) > -1) {
      const completeChunk = partialChunk.slice(0, belIndex);
      partialChunk = partialChunk.slice(belIndex + 1);
      if (completeChunk) {
        try {
          const obj = JSON.parse(completeChunk);
          yield obj;
        } catch (e) {
          console.log("Chunk parsing error: ", e);
        }
      }
    }

    if (done) {
      break;
    }
  }
}

export const updateMessages = (
  messages: Message[],
  newMessage: Message
): Message[] => {
  return messages.map((message) => {
    if (
      (message.id === "-1" && newMessage.type === ChatType.User) ||
      (message.id === "-2" && newMessage.type === ChatType.Assistant)
    ) {
      return newMessage;
    }

    return message;
  });
};

export const createTempMessage = (content: string, type: ChatType): Message => {
  return {
    type: type,
    content: content,
    id: type === ChatType.User ? "-1" : "-2",
    threadId: "",
    createdDate: new Date(Date.now()),
    lastModified: new Date(Date.now()),
    options: { isDisliked: false, isLiked: false },
  };
};
