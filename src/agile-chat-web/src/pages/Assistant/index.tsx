import { Button } from "@/components/ui/button"
import AssistantsComponent from "@/components/AssistantsComponent"
import SimpleHeading from '@/components/Heading-Simple'
import { useNavigate } from "react-router-dom"

const AssistantsPage = () => {
   
  const navigate = useNavigate();

  const handleNewAssistant = () => {
    console.log("Connect to Database clicked")
     navigate("/assistant");  
  }

  return (
    <div className="flex h-screen bg-background text-foreground">

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading Title="AI Assistant" Subtitle='Configure your AI Assistants' DocumentCount={0} />

        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
            {/* CTA Buttons */}
            <Button 
                  className="bg-black text-white hover:bg-gray-800 h-12"
                  onClick={handleNewAssistant}
                >
                  New AI Assistant
                </Button>                            
            {/* Tools Component */}
            <AssistantsComponent />
          </main>
        </div>
      </div>
    </div>
  )
}

export default AssistantsPage