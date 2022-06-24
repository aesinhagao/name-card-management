import { StyleSheet } from "react-native";

const styles = StyleSheet.create({
    containerOverlay: {
        fontfamily: 'Roboto',
        flex: 1,
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: 'rgba(130, 130, 130,0.5)',
    },
    modalView: {
        backgroundColor: '#ffff',
        borderRadius: 10,
    },
    modalView_title: {
        justifyContent: 'center',
        alignItems: 'center',
        marginBottom: 10,
    },
    modalView_title_label: {
        fontSize: 16,
        fontWeight: 'bold',
    },
    modalView_item: {
        width: '70%',
    },
    modalView_item_content: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: 5,
    },
    modalView_item_content_label: {
        fontSize: 14,
        width: '90%',
    }
})

export default styles;