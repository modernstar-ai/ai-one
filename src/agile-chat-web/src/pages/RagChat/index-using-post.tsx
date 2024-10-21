import { useState } from 'react';
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { ScrollArea } from "@/components/ui/scroll-area";
import SidebarMenu from '@/components/Sidebar'
import SimpleHeading from '@/components/Heading-Simple';
import { getRagApiUri } from '@/services/uri-helpers';
import SearchResultComponent from '@/components/Search-Result';

interface SearchMessage {
  content: string;
  role: string;
  thoughtProcess: string;
  citations: string[];
  followUpQuestions?: string[];
}

// Function to create a new message with default values
//Purpose: The createMessage function is a utility function used to create a new message object with default values for certain properties.
//Usage: It is used to ensure that every new message has default values for properties like thoughtProcess and citations, even if they are not explicitly provided.
const createMessage = (overrides: Partial<SearchMessage> = {}): SearchMessage => {
  console.log('createMessage overrides:', overrides);
  return {
    content: 'How can I help you today?',
    role: 'welcome',
    thoughtProcess: '',
    citations: [],
    ...overrides,
  };
};

const RagChatPage = () => {
  const [messages, setMessages] = useState<SearchMessage[]>(() => [createMessage()]);
  const [inputValue, setInputValue] = useState("");



  //  Purpose: The addSearchMessage function is used to add a new message to the state. It uses the createMessage function to create the new message with default values and then updates the state.
  //  Usage: It is a higher-level function that abstracts the process of adding a new message to the state, ensuring that the new message is created with default values.
  // const addSearchMessageToState = (newMessage: Partial<SearchMessage>) => {
  //   console.log('ragchat state update start newMessage:',messages?.length ,newMessage, 'messages:', messages);
  //   setMessages([...messages, createMessage(newMessage)]);
  //   console.log('ragchat state update end newMessage:', messages?.length ,newMessage, 'messages:', messages);
  // };

  const handleSendMessage = async () => {

    console.log('ragchat inputValue:', inputValue);

    if (inputValue.trim()) {
      // Add the user's message to the chat. we use this for binding.
      const newMessage = createMessage({ content: inputValue, role: "user" });
      const newMessages = [...messages, newMessage];
      console.log('ragchat newMessages:', newMessages);
      console.log('ragchat messages - click:', messages?.length, 'messages:', messages);
      //update the state
      // addSearchMessageToState(newMessage);
      // Add the bot's response to the chat
      setMessages((prevMessages) => [
        ...prevMessages,
        newMessage
      ]);
      console.log('ragchat messages - add search messages to state:', messages?.length, 'messages:', messages);


      const apiUrl = getRagApiUri('chatoverdata');
      console.log('ragchat apiUrl:', apiUrl);
      console.log('ragchat messages:', messages);
      console.log('ragchat call api with newMessages:', newMessages);

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
        console.log('ragchat messages - data returned:', messages?.length, 'messages:', messages);

        // Add the bot's response to the chat
        // setMessages((prevMessages) => [
        //   ...prevMessages,
        //   { content: data.response, role: "assistant" }
        // ]);

        // Extract the relevant details from the response
        if (data.choices && data.choices.length > 0) {
          const choice = data.choices[0];
          const assistantMessage = choice.message.content;
          const followUpQuestions = choice?.followup_questions ?? [];
          const throughProcess = choice.thoughts?.description ?? "";
          // Extract citations from data.choices
          //const citations = []//data.choices.map((choice) => choice.message.content);

          // Add the bot's response to the chat
          console.log('ragchat messages - before add search message to state:', messages?.length, 'messages:', messages);

          // addSearchMessageToState({
          //   role: "assistant",
          //   content: assistantMessage,
          //   thoughtProcess: throughProcess,
          //   citations: [],
          //   followUpQuestions: followUpQuestions
          // });
          setMessages((prevMessages) => [
            ...prevMessages,
            createMessage({ role: "assistant", content: assistantMessage, thoughtProcess: throughProcess, citations: [], followUpQuestions: followUpQuestions })
          ]);
          console.log('ragchat messages - after add search message to state:', messages?.length, 'messages:', messages);

          // Optionally, you can use choice.context and other details if you want to display more data
          if (choice.context) {
            console.log("Context Data Points:", choice.context.dataPointsContent);
            console.log("Thoughts:", choice.context.thoughts);
          }
        }



      } catch (error) {
        console.error("Fetch error:", error);
        //addSearchMessageToState({ content: "Error: Unable to fetch response from the server.", role: "assistant" });
        setMessages((prevMessages) => [
          ...prevMessages,
          createMessage({ role: "assistant", content: "Error: Unable to fetch response from the server."})
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
      <SidebarMenu />


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
              {message.role === "assistant" && (
                <SearchResultComponent
                  Answer={message.content || ""}
                  ThoughtProcess={message.thoughtProcess || ""}
                  Citations={message.citations || []}
                  FollowUpQuestions={message.followUpQuestions || []}
                  SupportingContent={message.followUpQuestions || []}
                />

              )}
              {message.role != "assistant" && (
                <div
                  className={`p-6 rounded-lg`}
                  style={{
                    backgroundColor: message.role === "user" ? "var(--chat-user-bg-color)" : "var(--chat-bot-bg-color)",
                    color: message.role === "user" ? "var(--chat-user-text-color)" : "var(--chat-bot-text-color)",
                    width: "60%"
                  }}
                >
                  {message.content}
                  <div><small>{JSON.stringify(message)}</small></div>
                </div>
              )}

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
