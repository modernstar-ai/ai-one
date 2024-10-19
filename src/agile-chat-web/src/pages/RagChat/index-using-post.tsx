import { useState } from 'react';
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { ScrollArea } from "@/components/ui/scroll-area";
import LeftMenu from '@/components/Menu-Left';
import SimpleHeading from '@/components/Heading-Simple';
import { getRagApiUri } from '@/services/uri-helpers';

const RagChatPage = () => {
  const [messages, setMessages] = useState<{ content: string; role: string }[]>([]);
  const [inputValue, setInputValue] = useState("");

  const [isHistoryOpen, setIsHistoryOpen] = useState(false);

  const handleSendMessage = async () => {

    console.log('ragchat inputValue:', inputValue);

    if (inputValue.trim()) {
      // Add the user's message to the chat
      const newMessages = [...messages, { content: inputValue, role: "user" }];
      setMessages(newMessages);
      console.log('ragchat newMessages:', newMessages);


      const apiUrl = getRagApiUri('chatoverdata');
      console.log('ragchat apiUrl:', apiUrl);
      console.log('ragchat messages:', messages);
      console.log('ragchat newMessages:', newMessages);

      try {
        const response = await fetch(apiUrl, {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(newMessages),
        });

        if (!response.ok) {
          throw new Error("Network response was not ok");
        }

        const data = await response.json();
        console.log('ragchat data:', data);

        // Add the bot's response to the chat
        // setMessages((prevMessages) => [
        //   ...prevMessages,
        //   { content: data.response, role: "assistant" }
        // ]);

         // Extract the relevant details from the response
         if (data.choices && data.choices.length > 0) {
          const choice = data.choices[0];
          const assistantMessage = choice.message.content;

          // Add the bot's response to the chat
          setMessages((prevMessages) => [
              ...prevMessages,
              { role: "assistant", content: assistantMessage }
          ]);

          // Optionally, you can use choice.context and other details if you want to display more data
          if (choice.context) {
              console.log("Context Data Points:", choice.context.dataPointsContent);
              console.log("Thoughts:", choice.context.thoughts);
          }
      }



      } catch (error) {
        console.error("Fetch error:", error);
        setMessages((prevMessages) => [
          ...prevMessages,
          { content: "Error: Unable to fetch response from the server.", role: "assistant" }
        ]);

      }
      // Clear the input field after sending the message
      console.log('clearing inputValue');
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
        <SimpleHeading Title="Chat Over Your Data" Subtitle="Let's have a chat to your data " DocumentCount={0} />


        <ScrollArea className="flex-1 p-4 space-y-4">
          {messages.map((message, index) => (
            <div
              key={index}
              className={`w-full my-4 p-4 flex ${message.role === "user" ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`p-6 rounded-lg`}
                style={{
                  backgroundColor: message.role === "user" ? "var(--chat-user-bg-color)" : "var(--chat-bot-bg-color)",
                  color: message.role === "user" ? "var(--chat-user-text-color)" : "var(--chat-bot-text-color)",
                  width: "60%"
                }}
              >
                {message.content}
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
          <Button onClick={handleSendMessage}  >Send</Button>
        </div>
      </div>
    </div>
  );
};

export default RagChatPage;
