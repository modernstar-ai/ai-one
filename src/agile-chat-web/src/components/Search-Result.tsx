import { ScrollArea } from "@/components/ui/scroll-area"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { FileText, Lightbulb, BookOpen } from "lucide-react"

interface SearchResultProps {
    Answer: string;
    ThoughtProcess: string;
    Citations: string[];
    FollowUpQuestions: string[];
    SupportingContent: string[];
}

const SearchResultComponent: React.FC<SearchResultProps> = ({ Answer, ThoughtProcess, Citations, FollowUpQuestions, SupportingContent }) => {


    return (
        <Card className="w-full max-w-3xl">
            <CardContent className="p-6">
                <Tabs defaultValue="answer" className="w-full">
                    <TabsList className="grid w-full grid-cols-3">
                        <TabsTrigger value="answer" className="flex items-center justify-center gap-2">
                            <FileText className="h-4 w-4" />
                            <span className="whitespace-nowrap">Answer</span>
                        </TabsTrigger>
                        <TabsTrigger value="thought-process" className="flex items-center justify-center gap-2">
                            <Lightbulb className="h-4 w-4" />
                            <span className="whitespace-nowrap">Thought Process</span>
                        </TabsTrigger>
                        <TabsTrigger value="supporting-content" className="flex items-center justify-center gap-2">
                            <BookOpen className="h-4 w-4" />
                            <span className="whitespace-nowrap">Supporting Content</span>
                        </TabsTrigger>
                    </TabsList>
                    <div className="mt-6 border rounded">
                        <ScrollArea className="h-[400px] w-full">
                            <div className="p-4">
                                <TabsContent value="answer" className="mt-0 data-[state=inactive]:hidden">
                                    <h3 className="text-lg font-semibold mb-4">Answer</h3>
                                    <p className="mb-4">
                                        {Answer}
                                    </p>
                                    <h4 className="font-semibold mt-6 mb-2">Citations:</h4>
                                    <ul className="list-disc pl-5 space-y-1">

                                        {Citations && Citations.length > 0 ? (
                                            Citations.map((citation, index) => (
                                                <li key={index}>
                                                    <Button variant="link" className="h-auto p-0 text-blue-500">
                                                        {citation}
                                                    </Button>
                                                </li>
                                            ))
                                        ) : (
                                            <li>No citations available</li>
                                        )}

                                    </ul>
                                    <h4 className="font-semibold mt-6 mb-2">Follow-up questions:</h4>
                                    <ul className="list-disc pl-5 space-y-1 text-blue-500">
                                        {FollowUpQuestions && FollowUpQuestions.length > 0 ? (
                                            FollowUpQuestions.map((question, index) => (
                                                <li key={index}>{question}</li>
                                            ))
                                        ) : (
                                            <li>No follow up questions.</li>
                                        )}
                                    </ul>
                                </TabsContent>
                                <TabsContent value="thought-process" className="mt-0 data-[state=inactive]:hidden">
                                    <h3 className="text-lg font-semibold mb-4">Thought Process</h3>
                                    <p>{ThoughtProcess}</p>
                                </TabsContent>
                                <TabsContent value="supporting-content" className="mt-0 data-[state=inactive]:hidden">
                                    <h3 className="text-lg font-semibold mb-4">Supporting Content</h3>
                                    <ul className="list-disc pl-5 space-y-1 text-blue-500">
                                        {SupportingContent && SupportingContent.length > 0 ? (
                                            SupportingContent.map((question, index) => (
                                                <li key={index}>{question}</li>
                                            ))
                                        ) : (
                                            <li>No supporting content available</li>
                                        )}
                                    </ul>
                                </TabsContent>
                            </div>
                        </ScrollArea>
                    </div>
                </Tabs>
            </CardContent>
        </Card>
    )
}

export default SearchResultComponent;