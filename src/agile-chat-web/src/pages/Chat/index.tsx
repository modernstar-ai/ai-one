import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Book, Paperclip } from 'lucide-react'
import LeftMenu from '@/components/Menu-Left';
import SimpleHeading from '@/components/Heading-Simple';

const ChatPage = () => {
  // State for managing chat messages
  const [messages, setMessages] = useState<{ text: string; sender: string }[]>([]);
  const [inputValue, setInputValue] = useState("");
  const [isHistoryOpen, setIsHistoryOpen] = useState(false);

  // Function to handle sending a message
  const handleSendMessage = () => {
    if (inputValue.trim()) {
      // Add user message to the chat
      const newMessages = [...messages, { text: inputValue, sender: "user" }];
      setMessages(newMessages);

      // Simulate bot response
      setTimeout(() => {
        setMessages((prevMessages) => [
          ...prevMessages,
          { text: "response", sender: "bot" },
        ]);
      }, 500); // Simulate delay for the bot response

      // Clear input
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
        <SimpleHeading Title="Chat" Subtitle='Making your data fun again' DocumentCount={0} />

        {/* Chat Messages */}
        <ScrollArea className="flex-1 p-4 space-y-4">
          {messages.map((message, index) => (
            <div
              key={index}
              className={`w-full flex ${
                message.sender === "user"
                  ? "justify-end"  // Align user messages to the right
                  : "justify-start"  // Align bot messages to the left
              }`}
            >
              <div
                className={`max-w-xs p-3 rounded-lg`}
                style={{
                  backgroundColor: message.sender === "user" ? "var(--chat-user-bg-color)" : "var(--chat-bot-bg-color)",
                  color: message.sender === "user" ? "var(--chat-user-text-color)" : "var(--chat-bot-text-color)"
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
          />
          <div className="flex justify-between items-center">
            <div className="flex space-x-2">
              <Button variant="outline" size="icon"><Paperclip className="h-4 w-4" /></Button>
              <Button variant="outline" size="icon"><Book className="h-4 w-4" /></Button>
            </div>
            <Button onClick={handleSendMessage}><Book className="h-4 w-4 mr-2" />Send</Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatPage;
