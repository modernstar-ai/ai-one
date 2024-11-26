import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { ScrollArea } from "@/components/ui/scroll-area";
import SimpleHeading from "@/components/Heading-Simple";
import { getApiUri } from "@/services/uri-helpers";
import axios from "@/error-handling/axiosSetup";
import MessageContent from "@/components/chat-page/message-content";
import { ChatMessageArea } from "@/components/chat-page/chat-message-area";
import { useAuth } from "@/services/auth-helpers";
import {
  createChatThread,
  GetChatThreadMessages,
} from "@/services/chatthreadservice";
import { fetchAssistantById } from "@/services/assistantservice";
import { Message } from "@/types/ChatThread";
import { Assistant } from "@/types/Assistant";
import { AxiosError } from "axios";

const ChatPage = () => {
  const { "*": chatThreadId } = useParams();
  const urlParams = new URLSearchParams(window.location.search);
  const assistantId = urlParams.get("assistantId");
  const navigate = useNavigate();
  const { username } = useAuth();

  const [inputValue, setInputValue] = useState("");
  const [messages, setMessages] = useState<Message[]>([]);
  const [assistant, setAssistant] = useState<Assistant | null>(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isMessagesLoading, setIsMessagesLoading] = useState(false);
  const [isStreaming, setIsStreaming] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Separate assistant fetching logic
  const fetchAssistant = async (id: string) => {
    try {
      const fetchedAssistant = await fetchAssistantById(id);
      if (fetchedAssistant) {
        setAssistant(fetchedAssistant);
        console.log("Assistant loaded:", fetchedAssistant);
        return fetchedAssistant;
      }
      throw new Error("Assistant not found");
    } catch (err) {
      console.error("Error fetching assistant:", err);
      setError("Failed to load assistant");
      return null;
    }
  };

  // Initialize chat thread or load existing thread
  useEffect(() => {
    let isMounted = true;

    const initializeChatThread = async () => {
      try {
        setIsLoading(true);
        let currentAssistant = null;

        // Step 1: Fetch assistant if ID is provided
        if (assistantId && isMounted) {
          currentAssistant = await fetchAssistant(assistantId);
          console.log("Chat - currentAssistant:", currentAssistant);
          if (!currentAssistant) {
            throw new Error("Failed to load assistant");
          }
        }

        // Step 2: Handle chat thread initialization when no chatThreadId is provided
        if (!chatThreadId && isMounted) {
          const newThread = await createChatThread({
            name: "New Chat",
            userId: username,
            personaMessage: currentAssistant?.systemMessage || "",
            personaMessageTitle: currentAssistant?.name || "",
          });
          console.log("Chat - newThread:", newThread);

          if (!newThread) {
            throw new Error("Failed to create new chat thread");
          }

          const newUrl = assistantId
            ? `/chat/${newThread.id}?assistantId=${assistantId}`
            : `/chat/${newThread.id}`;
          console.log("Chat - newUrl to redirect to:", newUrl);

          navigate(newUrl, { replace: true });
        }
      } catch (err) {
        if (isMounted) {
          console.error("Chat initialization error:", err);
          setError(
            err instanceof Error
              ? err.message
              : "An error occurred while initializing chat"
          );
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    initializeChatThread();

    return () => {
      isMounted = false;
      setAssistant(null);
    };
  }, [assistantId, username, navigate]);

  // Effect to handle message fetching after chatThreadId is updated
  useEffect(() => {
    const fetchMessages = async () => {
      if (chatThreadId) {
        setIsMessagesLoading(true);

        const getMessages = await GetChatThreadMessages(
          username,
          chatThreadId,
          assistant
        );
        console.log("Chat - getMessages after new thread:", getMessages);

        // Parse and manage AssistantMessageContent
        const parsedMessages = getMessages.map((message) => {
          if (
            message.role === "assistant" &&
            typeof message.content === "string"
          ) {
            try {
              const parsedContent = JSON.parse(message.content); // Parse AssistantMessageContent JSON
              console.log("parsedContent:", parsedContent);
              return {
                ...message,
                content: parsedContent.response || message.content, // Display `response` field or fallback to raw content
                citations: parsedContent.citations || [], // Handle citations if included
              };
            } catch (err) {
              console.error("Failed to parse AssistantMessageContent:", err);
            } finally {
              setIsMessagesLoading(false);
            }
          }
          return message; // Return unmodified message for non-assistant roles
        });
        setMessages(parsedMessages);
      }
    };

    fetchMessages();
  }, [chatThreadId, username, assistant]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    if (!inputValue.trim() || !chatThreadId) return;

    const newMessage: Message = {
      id: crypto.randomUUID(),
      createdAt: new Date(),
      type: "text",
      isDeleted: false,
      content: inputValue,
      name: username,
      role: "user",
      threadId: chatThreadId,
      userId: username,
      multiModalImage: "",
      sender: "user",
      like: false,
      disLike: false,
    };

    // Add user message to chat
    setMessages((prev) => [...prev, newMessage]);
    setIsStreaming(true);

    try {
      // Use new /chat endpoint
      const apiUrl = getApiUri("chat", {
        threadId: chatThreadId,
        ...(assistantId && { assistantId }),
      });

      console.log("Chat apiUrl: ", apiUrl);

      // Create message history format
      const messageHistory = messages.map((msg) => ({
        text: msg.content,
        role: msg.role,
      }));

      // Add current message
      messageHistory.push({
        text: inputValue,
        role: "user",
      });

      // Clear input after storing message
      setInputValue("");

      // Create placeholder for bot response
      const botMessage: Message = {
        id: crypto.randomUUID(),
        createdAt: new Date(),
        type: "text",
        isDeleted: false,
        content: "",
        name: "Assistant",
        role: "assistant",
        threadId: chatThreadId,
        userId: username,
        multiModalImage: "",
        sender: "assistant",
        like: false,
        disLike: false,
      };
      setMessages((prev) => [...prev, botMessage]);

      const response = await axios.post(apiUrl, messageHistory, {
        headers: {
          Accept: "text/plain",
          "Content-Type": "application/json",
        },
        responseType: "stream",
        adapter: "fetch",
      });

      // // Handle streaming response
      const stream = response.data as ReadableStream;
      const reader = stream.getReader();
      const decoder = new TextDecoder("utf-8");

      let content = ""; // Store the content

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        content += chunk;

        console.log("content:", content);

        // Update the message incrementally for partial updates
        setMessages((prev) => {
          const lastMessage = prev[prev.length - 1];
          const updatedMessage = {
            ...lastMessage,
            content: content.trim(), // Partial content
          };
          return [...prev.slice(0, prev.length - 1), updatedMessage];
        });
      }

      // Finalize message with parsed response
      const responseJson = JSON.parse(content);
      const { response: botContent, citations } = responseJson;
      console.log("Bot content:", botContent);

      // Update the last message with the new content
      setMessages((prev) => {
        const lastMessage = prev[prev.length - 1];
        const updatedMessage = {
          ...lastMessage,
          content: botContent || "No content available", // Only update with content, not the entire JSON
          citations: citations || [], // Add citations if available
        };
        return [...prev.slice(0, prev.length - 1), updatedMessage];
      });
    } catch (err) {
      let message = "failed to send message";
      const axiosErr = err as AxiosError;
      const stream = axiosErr.response?.data as ReadableStream;
      if (stream) {
        const read = await stream.getReader().read();
        message = new TextDecoder("utf-8")
          .decode(read.value)
          .replace(/^"(.*)"$/, "$1");
      }
      console.error("Error sending message:", err);
      setError(message);
    } finally {
      setIsStreaming(false);
      setInputValue("");
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        Loading...
      </div>
    );
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      <div className="flex-1 flex flex-col">
        <SimpleHeading
          Title={assistant ? assistant.name : "Chat"}
          Subtitle={assistant ? assistant.description : "Why not have a chat"}
          DocumentCount={messages.length}
          threadId={chatThreadId}
        />

        {error && (
          <div className="p-4 bg-red-100 text-red-700 rounded-md m-4">
            {error}
          </div>
        )}

        <ScrollArea className="flex-1 p-4 space-y-4">
          {isMessagesLoading ? (
            <div className="flex justify-center items-center h-full">
              Loading messages...
            </div>
          ) : (
            messages.map(
              (message, index) => (
                //message.sender !== 'system' && (
                <ChatMessageArea
                  key={index}
                  messageId={message.id}
                  userId={username || ""} // Ensure username is never undefined
                  profileName={
                    message.sender === "user"
                      ? username || "User"
                      : assistant?.name || "AI Assistant"
                  }
                  role={message.sender === "user" ? "user" : "assistant"}
                  onCopy={() => {
                    navigator.clipboard.writeText(message.content);
                  }}
                  profilePicture={message.sender === "user" ? "" : "/agile.png"}
                  initialLikes={message.like}
                  initialDislikes={message.disLike}
                >
                  <MessageContent
                    message={{
                      role: message.sender === "user" ? "user" : "assistant",
                      content: message.content,
                      name: message.sender,
                      citations: message.citations,
                    }}
                  />
                </ChatMessageArea>
              )
              //)
            )
          )}
        </ScrollArea>

        <div className="p-4 border-t">
          <Textarea
            placeholder="Type your message here..."
            className="w-full mb-2"
            rows={4}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={handleKeyDown}
            autoFocus
            aria-label="Chat Input"
            accessKey="i"
          />

          <Button
            onClick={handleSendMessage}
            disabled={isStreaming || !chatThreadId || isMessagesLoading}
            aria-label="Send Chat"
            accessKey="j"
          >
            {isStreaming ? "Sending..." : "Send"}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
