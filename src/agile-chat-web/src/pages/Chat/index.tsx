import { useState } from 'react';
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { ScrollArea } from "@/components/ui/scroll-area";
import LeftMenu from '@/components/Menu-Left';
import SimpleHeading from '@/components/Heading-Simple';
import { getApiUri } from '@/services/uri-helpers';

const ChatPage = () => {
  const [messages, setMessages] = useState<{ text: string; sender: string }[]>([]);
  const [inputValue, setInputValue] = useState("");
  const [isStreaming, setIsStreaming] = useState(false);
    const [isHistoryOpen, setIsHistoryOpen] = useState(false);

  const handleSendMessage = async () => {
    if (inputValue.trim()) {
      // Add the user's message to the chat
      const newMessages = [...messages, { text: inputValue, sender: "user" }];
      setMessages(newMessages);
      setIsStreaming(true); // Set the streaming flag to true

      // Establish an SSE connection for the bot's response
      const apiUrl = getApiUri('chatcompletions',{prompt:encodeURIComponent(inputValue)});
      const eventSource = new EventSource(apiUrl);

       // Add the bot's message placeholder to the chat
       setMessages((prevMessages) => [
        ...prevMessages,
        { text: "", sender: "bot" } // Start with an empty message for the bot
      ]);

      eventSource.onmessage = (event) => {
        // Update the last bot message with the streamed data
        setMessages((prevMessages) => {
          const lastMessage = prevMessages[prevMessages.length - 1];
          const updatedBotMessage = { ...lastMessage, text: lastMessage.text + event.data };
          return [...prevMessages.slice(0, prevMessages.length - 1), updatedBotMessage];
        });
      };

      eventSource.onerror = (error) => {
        console.error("EventSource failed:", error);
        eventSource.close();
        setIsStreaming(false); // End the streaming
      };

      // Clear the input field after sending the message
      setInputValue("");
    }
  };

  return (
    <div className="flex h-screen bg-background text-foreground">


      {/* Left Sidebar */}
      <LeftMenu isHistoryOpen={isHistoryOpen} setIsHistoryOpen={setIsHistoryOpen} />

      {/* Search History Panel */}
      {isHistoryOpen && (
        <div className="w-64 bg-secondary p-4 overflow-auto">
          <h2 className="text-lg font-semibold mb-4">Search History</h2>
          <ScrollArea className="h-[calc(100vh-2rem)]">
            {/* Add your search history items here */}
            <div className="space-y-2">
              <div className="p-2 hover:bg-accent rounded">Previous search 1</div>
              <div className="p-2 hover:bg-accent rounded">Previous search 2</div>
              {/* ... more items ... */}
            </div>
          </ScrollArea>
        </div>
      )}

      {/* Main Chat Area */}
      <div className="flex-1 flex flex-col">
      {/* Header */}
      <SimpleHeading Title="Chat" Subtitle='Why not have a chat' DocumentCount={0} />


        <ScrollArea className="flex-1 p-4 space-y-4">
          {messages.map((message, index) => (
            <div
              key={index}
              className={`w-full my-4 p-4 flex ${message.sender === "user" ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`p-6 rounded-lg`}
                style={{
                  backgroundColor: message.sender === "user" ? "var(--chat-user-bg-color)" : "var(--chat-bot-bg-color)",
                  color: message.sender === "user" ? "var(--chat-user-text-color)" : "var(--chat-bot-text-color)",
                  width: "60%"
                }}
              >
                {message.text}
              </div>
            </div>
          ))}
        </ScrollArea>
        {/* Input Area */}
        <div className="p-4 border-t">
          <Textarea
            placeholder="Type your message here..."
            className="w-full mb-2"
            rows={4}
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            autoFocus
            aria-label="Chat Input"
            accessKey="i"
          />
          <Button onClick={handleSendMessage} disabled={isStreaming} aria-label="Send Chat" accessKey="j" >Send</Button>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
