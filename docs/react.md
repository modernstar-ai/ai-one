# React tips

## Arrow Functions vs Function Declarations

You can declare pages and components either way.
Arrow functions are more modern and often more terse. 

### Arrow Function

```jsx
import React, { useState } from 'react';

const PersonasPage = () => {
    const [isHistoryOpen, setIsHistoryOpen] = useState(false);

    return (
        <div>
            <div>Personas</div>
            <button onClick={() => setIsHistoryOpen(!isHistoryOpen)}>
                Toggle History
            </button>
            {isHistoryOpen && <div>History is open</div>}
        </div>
    );
};

export default PersonasPage;


```

if it just returns jsx it can be even shorter

```jsx
const PersonasPage = () => <div>Personas</div>;

```

### Function Declaration Component

```jsx
import React, { useState } from 'react';

export default function ChatInterface() {
    const [isHistoryOpen, setIsHistoryOpen] = useState(false);

    return (
        <div>
            <div>Personas</div>
            <button onClick={() => setIsHistoryOpen(!isHistoryOpen)}>
                Toggle History
            </button>
            {isHistoryOpen && <div>History is open</div>}
        </div>
    );
}

```