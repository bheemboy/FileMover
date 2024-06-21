<!-- <svelte:options accessors /> -->

<script>
    import FileButton from "./FileButton.svelte";

    export let BackendService;
    export let PathSettings;
    export let SelectedTab;
    export let CurrentFolderIndex;
    export let busy;

    $: FolderPath = PathSettings[SelectedTab].Paths[CurrentFolderIndex];
    let SelectedItem = null;
    let FolderContents = null;

    $: diable_copy_delete = !SelectedItem;
    $: disable_create_sha256 = !enableCreateSHA256(SelectedItem);
    $: disable_verify_sha256 = !enableVerifySHA256(SelectedItem);
    
    $: getFolderContents(FolderPath);

    function enableCreateSHA256(SelectedItem) {
        if (!SelectedItem) return false;
        const filename = SelectedItem.Name.toUpperCase(); 
        if (!SelectedItem.IsDirectory && filename.endsWith(".SHA256")) return false;
        
        const checksumFileName = filename + '.SHA256';
        const listOfCorrespondingSHA256 = FolderContents.filter((content) => (!content.IsDirectory && content.Name.toUpperCase() === checksumFileName));

        return listOfCorrespondingSHA256.length<=0;
    }

    function enableVerifySHA256(SelectedItem) {
        if (!SelectedItem) return false;
        if (SelectedItem.IsDirectory) return false;
        return SelectedItem.Name.toUpperCase().endsWith(".SHA256");
    }

    export async function getFolderContents(path) {
        if (path) {
            FolderContents = await BackendService.getFolderContents(path);
        } else {
            FolderContents = [];
        }
    }

    async function copy2Dest(destFolderIndex) {
        if (!SelectedItem) {
            console.log("Select an item to copy");
        } else {
            const dest = PathSettings[SelectedTab].Paths[destFolderIndex];
            await BackendService.copyPathToFolder(FolderPath, JSON.stringify(SelectedItem), dest);
        }
    }

    async function deleteClicked(event) {
        if (!SelectedItem) {
            console.log("Select an item to delete");
        } else {
            if (confirm(`Delete '${SelectedItem.Name}' from '${FolderPath}'?`) == true) {
                if (await BackendService.deletePath(FolderPath, SelectedItem.Name)) {
                    SelectedItem = null;
                }
            }
        }
    }

    async function createChecksum(event) {
    }


    async function verifyChecksum(event) {
    }

    function formatBytes(bytes, decimals = 2) {
        if (bytes === 0) return "0 Bytes";

        const k = 1024;
        const dm = decimals < 0 ? 0 : decimals;
        const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

        const i = Math.floor(Math.log(bytes) / Math.log(k));

        return (
            parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i]
        );
    }
</script>

<div class="Folder-Container">
    
    <p class="Folder-Path">{FolderPath}</p>
    
    <select size="10" bind:value={SelectedItem}>
        {#if FolderContents && FolderContents.length > 0}
            {#each FolderContents as content, i}
                <option class={(content.IsDirectory? "icon-folder": "icon-file")} value={content}>{`${content.Name} (${formatBytes(content.Size)})`}</option>
            {/each}
        {/if}
    </select>

    <div class="Folder-Buttons-Container">
        <div>
            {#if CurrentFolderIndex > 0}
                <FileButton
                    Type="COPYTOLEFT"
                    bind:busy
                    bind:disable_button={diable_copy_delete}
                    on:click={e => copy2Dest(CurrentFolderIndex-1)}
                />
            {:else}
                <div style="width: 50px;"/>
            {/if}
        </div>

        <div>
            <FileButton 
                Type="CREATE-SHA256" 
                bind:busy 
                bind:disable_button={disable_create_sha256}
                on:click={createChecksum}
            />
            <FileButton 
                Type="VERIFY-SHA256" 
                bind:busy 
                bind:disable_button={disable_verify_sha256}
                on:click={verifyChecksum}
            />
            <FileButton 
                Type="DELETE" 
                bind:busy 
                bind:disable_button={diable_copy_delete}
                on:click={deleteClicked}
            />
        </div>

        <div>
            {#if CurrentFolderIndex < PathSettings[SelectedTab].Paths.length-1}
                <FileButton
                    Type="COPYTORIGHT"
                    bind:busy 
                    bind:disable_button={diable_copy_delete}
                    on:click={e => copy2Dest(CurrentFolderIndex+1)}
                />
            {:else}
                <div style="width: 50px;"/>
            {/if}
        </div>
    </div>
</div>

<style>
    .Folder-Container {
        padding: 25px 45px 10px 45px;
        margin: 25px;
        flex: 1 1 0;
        /* width: 0; */
        /* flex-grow: 1; */
        padding-top: 10px;
        background-color: #404040;
        border-radius: 10px;
        /* border: 1px solid green; */
        box-shadow: 2px 0px 5px rgba(0, 0, 0, 0.5);
    }

    .Folder-Path {
        color: lightgray;
        font-size: medium;
        margin-bottom: 0;
        /* border: 1px solid green; */
    }

    select {
        background-color: rgb(150, 150, 150);
        font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
        font-size: medium;
        width: 100%;
        min-width: 250px;
        border-radius: 3px; /* Rounded corners */
        /* border: 2px solid green; */
    }

    select:focus {
        outline: none;
    }

    .icon-folder:before {
        content: "\1F4C1"; /* Unicode for folder icon */
        margin-right: 3px;
    }
    .icon-file:before {
        content: "\1F4C4"; /* Unicode for file icon */
        margin-right: 3px;
    }    

    .Folder-Buttons-Container {
        display: flex;
        justify-content: space-between;
        /* border: 2px solid red; */
    }
</style>
