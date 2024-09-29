import React from 'react'
import { Button } from "@/components/ui/button"
import { Home, History, MessageSquare, Settings } from 'lucide-react'

interface LeftMenuProps {
    isHistoryOpen: boolean;
    setIsHistoryOpen: React.Dispatch<React.SetStateAction<boolean>>;
}

const LeftMenu: React.FC<LeftMenuProps> = ({ isHistoryOpen, setIsHistoryOpen }) => {


    return (
        <div className="w-16 bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4">
            <Button variant="ghost" size="icon"><Home className="h-6 w-6" /></Button>
            <Button variant="ghost" size="icon" onClick={() => setIsHistoryOpen(!isHistoryOpen)}>
                <History className="h-6 w-6" />
            </Button>
            <Button variant="ghost" size="icon"><MessageSquare className="h-6 w-6" /></Button>
            <Button variant="ghost" size="icon"><Settings className="h-6 w-6" /></Button>
        </div>
    );
};

export default LeftMenu;