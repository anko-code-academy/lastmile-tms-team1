import { Text, View, StyleSheet } from "react-native";

export default function DeliveriesScreen() {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Deliveries</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, alignItems: "center", justifyContent: "center" },
  title: { fontSize: 24, fontWeight: "bold" },
});
