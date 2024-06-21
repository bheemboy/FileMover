<script>
    import { writable } from 'svelte/store';

    // Store to keep the messages
    const messages = writable([]);

    export let BackendService;

    BackendService.on("Message", onMessage);
    function onMessage(message) {
        if (message.Type == 0) { // Log message
            if (message.Message.length > 0) {
                messages.update(currentMessages => [...currentMessages, message.Message]);
            } else {
                messages.update(currentMessages => []);
            }

        }
    }
  </script>
  
  <style>
    .debug-messages {
      font-family: monospace;
      white-space: pre-wrap;
      background: #1E1E1E;
      color: gray;
      /* border: 1px solid #ddd; */
      padding: 10px 40px 10px 40px ;
      margin: 10px 0;
    }
  </style>
  
  <div class="debug-messages">
    {#each $messages as message (message)}
      <div>{message}</div>
    {/each}
  </div>
  