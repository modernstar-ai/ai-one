import React from 'react'
import { Button } from "@/components/ui/button"
import { Menu, Paperclip} from 'lucide-react'

// interface SimpleHeadingProps {
//     DocumentCount: number;    
// }

const SimpleHeading: React.FC = () => {
    return (
        <div className="bg-muted p-4 flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold">Detailed Image Descriptions</h1>
            <p className="text-sm text-muted-foreground">Subtitle goes here</p>
          </div>
          <div className="flex space-x-2">
            <Button variant="outline" size="icon"><Menu className="h-4 w-4" /></Button>
            <Button variant="outline" size="icon"><Paperclip className="h-4 w-4 mr-1" />0</Button>
          </div>
        </div>
    );
};

export default SimpleHeading;