import React from 'react'
import { Button } from "@/components/ui/button"
import { Home, History, MessageSquare, Settings, VenetianMask, MessageSquareCode } from 'lucide-react'
import { Link } from 'react-router-dom';

interface LeftMenuProps {
    isHistoryOpen: boolean;
    setIsHistoryOpen: React.Dispatch<React.SetStateAction<boolean>>;
}

const LeftMenu: React.FC<LeftMenuProps> = ({ isHistoryOpen, setIsHistoryOpen }) => {


    return (
        <div className="w-16 bg-primary text-primary-foreground flex flex-col items-center py-4 space-y-4">
            <Link to="/"><Button variant="ghost" size="icon"><Home className="h-6 w-6" /></Button></Link>
            <Link to="/chat"><Button variant="ghost" size="icon"><MessageSquare className="h-6 w-6" /></Button></Link>
            <Link to="/ragchat"><Button variant="ghost" size="icon"><MessageSquareCode className="h-6 w-6" /></Button></Link>
            <Button variant="ghost" size="icon" onClick={() => setIsHistoryOpen(!isHistoryOpen)}>
                <History className="h-6 w-6" />
            </Button>
            <Link to="/personas"><Button variant="ghost" size="icon"><VenetianMask className="h-6 w-6" /></Button></Link>
            <Link to="/tools"><Button variant="ghost" size="icon"><Settings className="h-6 w-6" /></Button></Link>
        </div>
    );
};

export default LeftMenu;