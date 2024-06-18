<script>
    import Folder from "./Folder.svelte";

    export let BackendService;
    export let PathSettings;
    export let SelectedTab;
    export let busy;

    let FolderControls = [];
    if (PathSettings && SelectedTab> 0) {
        FolderControls = Array(PathSettings[SelectedTab].Paths.length);
    }

    BackendService.on("Message", onMessage);
    function onMessage(message) {
        if (message.Type == 4) {
            RefreshFolder(message.Folder);
        }
    }

    function RefreshFolder(path) {
        let index = PathSettings[SelectedTab].Paths.indexOf(path);
        if (index >= 0) {
            FolderControls[index].getFolderContents(path);
        }
    }

</script>

<div class="FolderList-Container">
    {#if PathSettings.length > 0 && SelectedTab >=0 }
        {#each PathSettings[SelectedTab].Paths as path, i}
            
            <Folder 
                bind:this={FolderControls[i]}    
                bind:BackendService 
                bind:PathSettings 
                bind:SelectedTab 
                bind:busy
                CurrentFolderIndex={i} 
            />
            
        {/each}
    {/if}
</div>

<style>

.FolderList-Container {
    display: flex;
    justify-content: space-evenly;
    flex-wrap: wrap;
    /* border: 1px solid brown; */
}

</style>
