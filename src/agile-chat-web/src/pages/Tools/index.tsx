import { Button } from "@/components/ui/button"
import ToolsComponent from "@/components/ToolsComponent"
import SidebarMenu from '@/components/Sidebar'
import SimpleHeading from '@/components/Heading-Simple'
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useNavigate } from "react-router-dom"


const ToolsPage = () => {
   
  const navigate = useNavigate();

  const handleConnectDatabase = () => {
    console.log("Connect to Database clicked")
     navigate("/connecttodb");  

  }
 
  const handleConnectLogicApp = () => {
    console.log("Connect to Logic App clicked")
    navigate("/connecttologicapp");  
  }

  const handleConnectExternalAPI = () => {
    console.log("Connect to External API clicked")
    navigate("/connecttoapi");  
  }

  return (
    <div className="flex h-screen bg-background text-foreground">
      {/* Left Sidebar */}
      <SidebarMenu />      

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <SimpleHeading Title="Tools" Subtitle='Configure your tools' DocumentCount={0} />

        <div className="flex-1 p-4 overflow-auto">
          <main className="flex-1 space-y-6">
            {/* CTA Buttons */}
            <Card>
              <CardHeader>
                <CardTitle>Add New Tools</CardTitle>
              </CardHeader>
              <CardContent className="flex flex-wrap gap-4">
                <Button 
                  className="bg-black text-white hover:bg-gray-800 h-12"
                  onClick={handleConnectDatabase}
                >
                  Connect to Database
                </Button>
                <Button 
                  className="bg-black text-white hover:bg-gray-800 h-12"
                  onClick={handleConnectLogicApp}
                >
                  Connect to Logic App
                </Button>
                <Button 
                  className="bg-black text-white hover:bg-gray-800 h-12"
                  onClick={handleConnectExternalAPI}
                >
                  Connect to External API
                </Button>
              </CardContent>
            </Card>
            
            {/* Tools Component */}
            <ToolsComponent />
          </main>
        </div>
      </div>
    </div>
  )
}

export default ToolsPage